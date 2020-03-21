using MediatR;

namespace MX.Images.Command.Sync
{
    public class SyncCommand
        : IRequest
    {
        public SyncCommand(string sourcePath, string destinationPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public string SourcePath { get; }

        public string DestinationPath { get; }
    }
}