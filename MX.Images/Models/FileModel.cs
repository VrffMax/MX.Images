using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.ObjectModel;

namespace MX.Images.Models
{
	public class FileModel
	{
		[BsonId] public ObjectId Id { get; set; }

		public string Machine { get; set; }

		public string Path { get; set; }

		public string Name { get; set; }

		public DateTime LastWriteTimeUtc { get; set; }

		public ReadOnlyCollection<FileModelTag> Tags { get; set; }
	}
}