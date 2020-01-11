using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class RepositoryCommand
        : IRequest
    {
        public RepositoryCommand(ReadOnlyCollection<FileModel> files) =>
            Files = files;

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}