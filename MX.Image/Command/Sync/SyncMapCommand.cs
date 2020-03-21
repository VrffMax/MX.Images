using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Model.CQRS;

namespace MX.Images.Command.Sync
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