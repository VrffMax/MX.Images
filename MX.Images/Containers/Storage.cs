using MongoDB.Driver;
using MX.Images.Interfaces;
using MX.Images.Models.Mongo;
using System;

namespace MX.Images.Containers
{
	public class Storage
		: IStorage
	{
		// ReSharper disable once InconsistentNaming
		private const string MX = nameof(MX);

		public Storage(IOptions options)
		{
			var dataBase = new MongoClient(options.MongoConnectionString)
				.GetDatabase(MX);

			Images = new Lazy<IMongoCollection<FileModel>>(() =>
				dataBase.GetCollection<FileModel>(nameof(IStorage.Images)));
		}

		public Lazy<IMongoCollection<FileModel>> Images { get; }
	}
}