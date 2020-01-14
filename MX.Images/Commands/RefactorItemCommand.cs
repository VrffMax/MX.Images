using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class RefactorItemCommand
        : IRequest
    {
        public RefactorItemCommand(FileModel file) =>
            File = file;

        public FileModel File { get; }
    }
}