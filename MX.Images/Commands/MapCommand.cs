using MediatR;
using MX.Images.Models;
using System.Collections.ObjectModel;

namespace MX.Images.Commands
{
    public class MapCommand
        : IRequest<ReadOnlyCollection<RefactorDirectoryModel>>
    {
        public MapCommand(string destinationPath, ReadOnlyCollection<RefactorItemModel> refactorItems)
        {
            DestinationPath = destinationPath;
            RefactorItems = refactorItems;
        }

        public string DestinationPath { get; }

        public ReadOnlyCollection<RefactorItemModel> RefactorItems { get; }
    }
}