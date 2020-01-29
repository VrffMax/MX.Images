using MediatR;
using MX.Images.Models.CQRS;

namespace MX.Images.Commands.Scan
{
	public class ScanDirectoryCommand
		: IRequest<DirectoryModel>
	{
		public ScanDirectoryCommand(string sourcePath) =>
			SourcePath = sourcePath;

		public string SourcePath { get; }
	}
}