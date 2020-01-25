using MediatR;
using MX.Images.Models.Mongo;
using System.Collections.ObjectModel;

namespace MX.Images.Commands.Scan
{
	public class RepositoryCommand
		: IRequest
	{
		public RepositoryCommand(ReadOnlyCollection<FileModel> files) =>
			Files = files;

		public ReadOnlyCollection<FileModel> Files { get; }
	}
}