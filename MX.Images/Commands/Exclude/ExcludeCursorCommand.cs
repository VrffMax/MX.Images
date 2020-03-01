using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models.Mongo;

namespace MX.Images.Commands.Exclude
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