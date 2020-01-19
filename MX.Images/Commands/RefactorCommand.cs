using MediatR;

namespace MX.Images.Commands
{
    public class RefactorCommand
        : IRequest
    {
        public RefactorCommand(string sourcePath, string destinationPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public string SourcePath { get; }

        public string DestinationPath { get; }
    }
}