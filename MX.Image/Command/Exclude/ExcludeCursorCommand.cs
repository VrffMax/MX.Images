using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Model.Mongo;

namespace MX.Images.Command.Exclude
{
    public class ExcludeCursorCommand
        : IRequest
    {
        public ExcludeCursorCommand(
            ReadOnlyCollection<FileModel> files,
            string sourcePath,
            string destinationPath)
        {
            Files = files;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public ReadOnlyCollection<FileModel> Files { get; }

        public string SourcePath { get; }

        public string DestinationPath { get; }
    }
}