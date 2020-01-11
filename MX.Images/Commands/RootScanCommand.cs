using MediatR;

namespace MX.Images.Commands
{
    public class RootScanCommand
        : IRequest
    {
        public RootScanCommand(string path) =>
            Path = path;

        public string Path { get; }
    }
}