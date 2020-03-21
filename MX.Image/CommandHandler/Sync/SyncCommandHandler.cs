using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Command.Sync;
using MX.Images.ContainerInterface;
using MX.Images.Model;
using MX.Images.Model.CQRS;
using MX.Images.Model.Mongo;

namespace MX.Images.CommandHandler.Sync
{
    public class SyncCommandHandler
        : IRequestHandler<SyncCommand>
    {
        private readonly IMediator _mediator;
        private readonly IOptions _options;
        private readonly IState _state;
        private readonly IStorage _storage;

        public SyncCommandHandler(
            IMediator mediator,
            IStorage storage,
            IOptions options,
            IState state)
        {
            _mediator = mediator;
            _storage = storage;
            _options = options;
            _state = state;
        }

        public async Task<Unit> Handle(SyncCommand request, CancellationToken cancellationToken)
        {
            var findFilter = Builders<FileModel>.Filter.Where(fileModel => true
                                                                           && fileModel.Machine == _options.Machine
                                                                           && fileModel.Path.StartsWith(
                                                                               request.SourcePath));

            var findOptions = new FindOptions<FileModel, FileModel>
            {
                Projection = Builders<FileModel>.Projection
                    .Include(fileModel => fileModel.Id)
                    .Include(fileModel => fileModel.Path)
                    .Include(fileModel => fileModel.Name)
                    .Include(fileModel => fileModel.LastWriteTimeUtc)
                    .Include(fileModel => fileModel.Tags)
            };

            var refactorCursorTasks = new Task<ReadOnlyCollection<RefactorItemModel>>[0];

            try
            {
                var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

                while (await filesCursor.MoveNextAsync(cancellationToken))
                    refactorCursorTasks = refactorCursorTasks.Append(_mediator.Send(
                        new SyncCursorCommand(Array.AsReadOnly(filesCursor.Current.ToArray())),
                        cancellationToken)).ToArray();

                await Task.WhenAll(refactorCursorTasks);

                var refactorDirectories = await _mediator.Send(
                    new SyncMapCommand(
                        request.DestinationPath,
                        Array.AsReadOnly(refactorCursorTasks.SelectMany(refactorItemTask => refactorItemTask.Result)
                            .ToArray())),
                    cancellationToken);

                var refactorCopyTasks = refactorDirectories.Select(refactorDirectory =>
                    _mediator.Send(new SyncCopyCommand(refactorDirectory), cancellationToken)).ToArray();

                await Task.WhenAll(refactorCopyTasks);

                return Unit.Value;
            }
            catch (Exception exception)
            {
                _state.Log(nameof(SyncCommandHandler), exception);

                return Unit.Value;
            }
        }
    }
}