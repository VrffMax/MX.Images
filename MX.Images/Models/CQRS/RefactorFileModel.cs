using MX.Images.Models.Mongo;
using System.Collections.ObjectModel;

namespace MX.Images.Models.CQRS
{
	public class RefactorFileModel
	{
		public string Name { get; set; }

		public ReadOnlyCollection<FileModel> Sources { get; set; }
	}
}