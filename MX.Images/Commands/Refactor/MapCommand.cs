using MediatR;
using MX.Images.Models.CQRS;
using System.Collections.ObjectModel;

namespace MX.Images.Commands.Refactor
{
	public class MapCommand
		: IRequest<ReadOnlyCollection<RefactorDirectoryModel>>
	{
		public MapCommand(string destinationPath, ReadOnlyCollection<RefactorItemModel> refactorItems)
		{
			DestinationPath = destinationPath;
			RefactorItems = refactorItems;
		}

		public string DestinationPath { get; }

		public ReadOnlyCollection<RefactorItemModel> RefactorItems { get; }
	}
}