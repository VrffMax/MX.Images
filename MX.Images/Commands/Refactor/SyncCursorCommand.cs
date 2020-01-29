using MediatR;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;
using System.Collections.ObjectModel;

namespace MX.Images.Commands.Refactor
{
	public class SyncCursorCommand
		: IRequest<ReadOnlyCollection<RefactorItemModel>>
	{
		public SyncCursorCommand(ReadOnlyCollection<FileModel> files) =>
			Files = files;

		public ReadOnlyCollection<FileModel> Files { get; }
	}
}