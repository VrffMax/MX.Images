using MediatR;

namespace MX.Images.Commands
{
    public class RefactorCommand
        : IRequest
    {
        public RefactorCommand(string path) =>
            Path = path;

        public string Path { get; }
    }
}