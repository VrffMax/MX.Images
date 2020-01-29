using System.Collections.ObjectModel;
using MediatR;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;

namespace MX.Images.Commands.Sync
{
	public class SyncCursorCommand
		: IRequest<ReadOnlyCollection<RefactorItemModel>>
	{
		public SyncCursorCommand(ReadOnlyCollection<FileModel> files) =>
			Files = files;

		public ReadOnlyCollection<FileModel> Files { get; }
	}
}