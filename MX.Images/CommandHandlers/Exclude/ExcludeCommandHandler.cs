using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Exclude;
using MX.Images.Interfaces;
using MX.Images.Models;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Exclude
{
    public class ExcludeCommandHandler
        : IRequestHandler<ExcludeCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly IOptions _options;
        private readonly IState _state;
        private readonly IStorage _storage;

        public ExcludeCommandHandler(
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

        public async Task<Unit> Handle(ExcludeCommand request, CancellationToken cancellationToken)
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
            };

            var excludeCursorTasks = new Task[0];

            try
            {
                var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

                while (await filesCursor.MoveNextAsync(cancellationToken))
                    excludeCursorTasks = excludeCursorTasks.Append(_mediator.Send(
                        new ExcludeCursorCommand(
                            Array.AsReadOnly(filesCursor.Current.ToArray()),
                            request.SourcePath,
                            request.DestinationPath),
                        cancellationToken)).ToArray();

                await Task.WhenAll(excludeCursorTasks);

                return Unit.Value;
            }
            catch (Exception exception)
            {
                _state.Log(nameof(ExcludeCommandHandler), exception);

                return Unit.Value;
            }
        }
    }
}