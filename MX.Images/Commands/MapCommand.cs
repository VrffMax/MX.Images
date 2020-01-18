using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class MapCommand
        : IRequest<ReadOnlyCollection<RefactorDirectoryModel>>
    {
        public MapCommand(string path, ReadOnlyCollection<RefactorItemModel> refactorItems)
        {
            Path = path;
            RefactorItems = refactorItems;
        }

        public string Path { get; }

        public ReadOnlyCollection<RefactorItemModel> RefactorItems { get; }
    }
}