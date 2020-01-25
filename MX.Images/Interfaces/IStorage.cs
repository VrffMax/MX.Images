using MongoDB.Driver;
using MX.Images.Models.Mongo;
using System;

namespace MX.Images.Interfaces
{
	public interface IStorage
	{
		Lazy<IMongoCollection<FileModel>> Images { get; }
	}
}