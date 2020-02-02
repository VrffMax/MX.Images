using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;

namespace MX.Images.Commands.Verify
{
    public class VerifyCursorCommand
        : IRequest
    {
        public VerifyCursorCommand(ReadOnlyCollection<FileModel> files) =>
            Files = files;

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}