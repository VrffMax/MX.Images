using MediatR;
using MX.Images.Commands;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
			var scan = await _mediator.Send(new DirectoryScanCommand(request.SourcePath), cancellationToken);

			var rootScanTasks = scan.Directories
				.Select(sourcePath => _mediator.Send(new RootScanCommand(sourcePath), cancellationToken))
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