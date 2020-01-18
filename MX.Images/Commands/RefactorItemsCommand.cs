using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models;

namespace MX.Images.Commands
{
    public class RefactorItemsCommand
        : IRequest<ReadOnlyCollection<RefactorItemModel>>
    {
        public RefactorItemsCommand(ReadOnlyCollection<FileModel> files) =>
            Files = files;

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}