using MediatR;
using MX.Images.Commands.Verify;
using System.Threading;
using System.Threading.Tasks;

namespace MX.Images.CommandHandlers.Verify
{
	public class VerifyCommandHandler
		: IRequestHandler<VerifyCommand, Unit>
	{
		public Task<Unit> Handle(VerifyCommand request, CancellationToken cancellationToken)
		{
			return Task.FromResult(Unit.Value);
		}
	}
}