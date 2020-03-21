using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Model.CQRS;
using MX.Images.Model.Mongo;

namespace MX.Images.Command.Sync
{
    public class SyncCursorCommand
        : IRequest<ReadOnlyCollection<RefactorItemModel>>
    {
        public SyncCursorCommand(ReadOnlyCollection<FileModel> files)
        {
            Files = files;
        }

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}