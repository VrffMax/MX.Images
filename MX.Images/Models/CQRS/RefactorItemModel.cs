using MX.Images.Models.Mongo;
using System;

namespace MX.Images.Models.CQRS
{
	public class RefactorItemModel
	{
		public string MakeModelDirectory { get; set; }

		public string Name { get; set; }

		public DateTime DateTime { get; set; }

		public FileModel File { get; set; }
	}
}