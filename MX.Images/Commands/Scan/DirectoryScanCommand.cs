using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands.Scan
{
    public class DirectoryScanCommand
        : IRequest<DirectoryModel>
    {
        public DirectoryScanCommand(string sourcePath) =>
            SourcePath = sourcePath;

        public string SourcePath { get; }
    }
}