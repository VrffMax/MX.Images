using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models.CQRS;

namespace MX.Images.Commands.Sync
{
    public class SyncMapCommand
        : IRequest<ReadOnlyCollection<RefactorDirectoryModel>>
    {
        public SyncMapCommand(string destinationPath, ReadOnlyCollection<RefactorItemModel> refactorItems)
        {
            DestinationPath = destinationPath;
            RefactorItems = refactorItems;
        }

        public string DestinationPath { get; }

        public ReadOnlyCollection<RefactorItemModel> RefactorItems { get; }
    }
}