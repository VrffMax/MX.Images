using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class RootScanCommandHandler
        : IRequestHandler<RootScanCommand>
    {
        private readonly IMediator _mediator;

        public RootScanCommandHandler(IMediator mediator) =>
            _mediator = mediator;

        public async Task<Unit> Handle(RootScanCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Path);

            var scan = await _mediator.Send(new DirectoryScanCommand(request.Path), cancellationToken);

            var rootScanTasks = scan.Directories
                .Select(path => _mediator.Send(new RootScanCommand(path), cancellationToken))
                .ToArray();

            var fileTasks = scan.Files
                .Select(file => _mediator.Send(new FileScanCommand(file), cancellationToken))
                .ToArray();

            await Task.WhenAll(
                Task.WhenAll(rootScanTasks),
                Task.WhenAll(fileTasks));

            await _mediator.Send(new RepositoryCommand(Array.AsReadOnly(fileTasks
                .Where(fileTask => fileTask.Result != default)
                .Select(fileTask => fileTask.Result).ToArray())), cancellationToken);

            return Unit.Value;
        }
    }
}