using MediatR;
using MX.Images.Model.CQRS;

namespace MX.Images.Command.Sync
{
    public class SyncCopyCommand
        : IRequest
    {
        public SyncCopyCommand(RefactorDirectoryModel refactorDirectory)
        {
            RefactorDirectory = refactorDirectory;
        }

        public RefactorDirectoryModel RefactorDirectory { get; }
    }
}