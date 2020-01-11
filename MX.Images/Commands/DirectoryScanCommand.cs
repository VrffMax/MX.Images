using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class DirectoryScanCommand
        : IRequest<DirectoryModel>
    {
        public DirectoryScanCommand(string path) =>
            Path = path;

        public string Path { get; }
    }
}