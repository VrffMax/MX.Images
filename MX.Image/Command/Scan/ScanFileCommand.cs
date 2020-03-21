using MediatR;
using MX.Images.Model.Mongo;

namespace MX.Images.Command.Scan
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