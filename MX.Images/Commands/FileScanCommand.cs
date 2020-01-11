using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class FileScanCommand
        : IRequest<FileModel>
    {
        public FileScanCommand(string file) =>
            File = file;

        public string File { get; }
    }
}