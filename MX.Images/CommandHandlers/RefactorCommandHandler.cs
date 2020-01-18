using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class RefactorCommandHandler
        : IRequestHandler<RefactorCommand>
    {
        private readonly IMediator _mediator;
        private readonly IStorage _storage;
        private readonly IOptions _options;

        public RefactorCommandHandler(
            IMediator mediator,
            IStorage storage,
            IOptions options)
        {
            _mediator = mediator;
            _storage = storage;
            _options = options;
        }

        public async Task<Unit> Handle(RefactorCommand request, CancellationToken cancellationToken)
        {
            var findFilter = Builders<FileModel>.Filter.Where(fileModel =>
                fileModel.Machine == _options.Machine);

            var findOptions = new FindOptions<FileModel, FileModel>
            {
                Projection = Builders<FileModel>.Projection
                    .Include(fileModel => fileModel.Id)
                    .Include(fileModel => fileModel.Path)
                    .Include(fileModel => fileModel.Name)
                    .Include(fileModel => fileModel.Tags)
            };

            var refactorItemTasks = new Task<ReadOnlyCollection<RefactorItemModel>>[0];
            var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

            while (await filesCursor.MoveNextAsync(cancellationToken))
            {
                refactorItemTasks = refactorItemTasks.Append(_mediator.Send(
                    new RefactorCursorCommand(Array.AsReadOnly(filesCursor.Current.ToArray())),
                    cancellationToken)).ToArray();
            }

            await Task.WhenAll(refactorItemTasks);

            var refactorDirectories = await _mediator.Send(
                new MapCommand(request.Path, Array.AsReadOnly(refactorItemTasks
                    .SelectMany(refactorItemTask => refactorItemTask.Result).ToArray())), cancellationToken);

            var refactorCopyTasks = refactorDirectories.Select(refactorDirectory =>
                _mediator.Send(new RefactorCopyCommand(refactorDirectory), cancellationToken)).ToArray();

            await Task.WhenAll(refactorCopyTasks);

            return Unit.Value;
        }
    }
}