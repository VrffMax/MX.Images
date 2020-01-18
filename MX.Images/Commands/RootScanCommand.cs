using MediatR;

namespace MX.Images.Commands
{
    public class RootScanCommand
        : IRequest
    {
        public RootScanCommand(string sourcePath) =>
            SourcePath = sourcePath;

        public string SourcePath { get; }
    }
}