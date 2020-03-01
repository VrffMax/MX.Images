using MediatR;

namespace MX.Images.Commands.Exclude
{
    public class ExcludeCommand
        : IRequest
    {
        public ExcludeCommand(string sourcePath, string destinationPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public string SourcePath { get; }
        
        public string DestinationPath { get; }
    }
}