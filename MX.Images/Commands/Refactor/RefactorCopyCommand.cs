using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands.Refactor
{
    public class RefactorCopyCommand
        : IRequest
    {
        public RefactorCopyCommand(RefactorDirectoryModel refactorDirectory) =>
            RefactorDirectory = refactorDirectory;

        public RefactorDirectoryModel RefactorDirectory { get; }
    }
}