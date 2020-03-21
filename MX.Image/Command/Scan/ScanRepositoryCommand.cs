using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Model.Mongo;

namespace MX.Images.Command.Scan
{
    public class ScanRepositoryCommand
        : IRequest
    {
        public ScanRepositoryCommand(ReadOnlyCollection<FileModel> files)
        {
            Files = files;
        }

        public ReadOnlyCollection<FileModel> Files { get; }
    }
}