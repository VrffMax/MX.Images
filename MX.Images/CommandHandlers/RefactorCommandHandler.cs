using System;
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
                    .Include(fileModel => fileModel.Tags)
            };

            var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

            while (await filesCursor.MoveNextAsync(cancellationToken))
            {
                var refactorItemTasks = filesCursor.Current.Select(file =>
                        _mediator.Send(new RefactorItemCommand(file), cancellationToken))
                    .ToArray();

                await Task.WhenAll(refactorItemTasks);
            }

            return Unit.Value;
        }
    }
}