using MediatR;
using MX.Images.Models.Mongo;

namespace MX.Images.Commands.Scan
{
    public class ScanFileCommand
        : IRequest<FileModel>
    {
        public ScanFileCommand(string file)
        {
            File = file;
        }

        public string File { get; }
    }
}