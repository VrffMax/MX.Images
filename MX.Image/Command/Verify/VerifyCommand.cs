using MediatR;

namespace MX.Images.Command.Verify
{
    public class VerifyCommand
        : IRequest
    {
        public VerifyCommand(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; }
    }
}