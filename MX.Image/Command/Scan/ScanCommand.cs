using MediatR;

namespace MX.Images.Command.Scan
{
    public class ScanCommand
        : IRequest
    {
        public ScanCommand(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; }
    }
}