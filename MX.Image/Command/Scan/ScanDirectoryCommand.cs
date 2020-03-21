using MediatR;
using MX.Images.Model.CQRS;

namespace MX.Images.Command.Scan
{
    public class ScanDirectoryCommand
        : IRequest<DirectoryModel>
    {
        public ScanDirectoryCommand(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; }
    }
}