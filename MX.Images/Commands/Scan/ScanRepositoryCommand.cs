using MediatR;
using MX.Images.Models.Mongo;
using System.Collections.ObjectModel;

namespace MX.Images.Commands.Scan
{
	public class ScanRepositoryCommand
		: IRequest
	{
		public ScanRepositoryCommand(ReadOnlyCollection<FileModel> files) =>
			Files = files;

		public ReadOnlyCollection<FileModel> Files { get; }
	}
}