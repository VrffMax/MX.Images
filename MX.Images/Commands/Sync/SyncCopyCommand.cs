using MediatR;
using MX.Images.Models.CQRS;

namespace MX.Images.Commands.Sync
{
	public class SyncCopyCommand
		: IRequest
	{
		public SyncCopyCommand(RefactorDirectoryModel refactorDirectory) =>
			RefactorDirectory = refactorDirectory;

		public RefactorDirectoryModel RefactorDirectory { get; }
	}
}