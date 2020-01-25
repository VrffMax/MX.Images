using MediatR;
using MX.Images.Models.CQRS;

namespace MX.Images.Commands.Scan
{
	public class DirectoryScanCommand
		: IRequest<DirectoryModel>
	{
		public DirectoryScanCommand(string sourcePath) =>
			SourcePath = sourcePath;

		public string SourcePath { get; }
	}
}