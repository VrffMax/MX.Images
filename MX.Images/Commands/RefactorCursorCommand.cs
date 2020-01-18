using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class RefactorCursorCommand
        : IRequest<ReadOnlyCollection<RefactorItemModel>>
    {
        public RefactorCursorCommand(ReadOnlyCollection<FileModel> files) =>
            Files = files;

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}