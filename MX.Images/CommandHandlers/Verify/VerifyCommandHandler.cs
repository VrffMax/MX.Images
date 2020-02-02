using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Verify;
using MX.Images.Interfaces;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Verify
{
    public class VerifyCommandHandler
        : IRequestHandler<VerifyCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly IStorage _storage;
        private readonly IOptions _options;

        public VerifyCommandHandler(
            IMediator mediator,
            IStorage storage,
            IOptions options)
        {
            _mediator = mediator;
            _storage = storage;
            _options = options;
        }

        public async Task<Unit> Handle(VerifyCommand request, CancellationToken cancellationToken)
        {
            var findFilter = Builders<FileModel>.Filter.Where(fileModel => true
                                                                           && fileModel.Machine == _options.Machine
                                                                           && fileModel.Path.StartsWith(
                                                                               request.SourcePath));

            var findOptions = new FindOptions<FileModel, FileModel>
            {
                Projection = Builders<FileModel>.Projection
                    .Include(fileModel => fileModel.Id)
                    .Include(fileModel => fileModel.CopyPath)
                    .Include(fileModel => fileModel.Hash)
            };

            var verifyCursorTasks = new Task[0];
            var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

            while (await filesCursor.MoveNextAsync(cancellationToken))
            {
                verifyCursorTasks = verifyCursorTasks.Append(_mediator.Send(
                    new VerifyCursorCommand(Array.AsReadOnly(filesCursor.Current.ToArray())),
                    cancellationToken)).ToArray();
            }

            await Task.WhenAll(verifyCursorTasks);

            return Unit.Value;
        }
    }
}