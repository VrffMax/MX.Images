using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Model.Mongo;

namespace MX.Images.Command.Verify
{
    public class VerifyCursorCommand
        : IRequest
    {
        public VerifyCursorCommand(ReadOnlyCollection<FileModel> files)
        {
            Files = files;
        }

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}