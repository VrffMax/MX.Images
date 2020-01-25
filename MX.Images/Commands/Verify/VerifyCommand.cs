using MediatR;

namespace MX.Images.Commands.Verify
{
	public class VerifyCommand
		: IRequest
	{
		public VerifyCommand(string sourcePath) =>
			SourcePath = sourcePath;

		public string SourcePath { get; }
	}
}