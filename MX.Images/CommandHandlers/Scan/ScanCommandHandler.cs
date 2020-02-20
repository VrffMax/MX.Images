using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands.Scan;

namespace MX.Images.CommandHandlers.Scan
{
    public class ScanCommandHandler
        : IRequestHandler<ScanCommand>
    {
        private readonly IMediator _mediator;

        public ScanCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(ScanCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Scan {request.SourcePath}");

            var scan = await _mediator.Send(new ScanDirectoryCommand(request.SourcePath), cancellationToken);

            var rootScanTasks = scan.Directories
                .Select(sourcePath => _mediator.Send(new ScanCommand(sourcePath), cancellationToken))
                .ToArray();

            var fileTasks = scan.Files
                .Select(file => _mediator.Send(new ScanFileCommand(file), cancellationToken))
                .ToArray();

            await Task.WhenAll(
                Task.WhenAll(rootScanTasks),
                Task.WhenAll(fileTasks));

            await _mediator.Send(new ScanRepositoryCommand(Array.AsReadOnly(fileTasks
                .Where(fileTask => fileTask.Result != default)
                .Select(fileTask => fileTask.Result).ToArray())), cancellationToken);

            return Unit.Value;
        }
    }
}