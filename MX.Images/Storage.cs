using System;
using MongoDB.Driver;

namespace MX.Images
{
    public class Storage
        : IStorage
    {
        private const string DefaultMongoConnectionString = "mongodb://localhost:27017";

        // ReSharper disable once InconsistentNaming
        private const string MX = nameof(MX);

        public Storage()
        {
            var dataBase = new MongoClient(DefaultMongoConnectionString).GetDatabase(MX);

            Images = new Lazy<IMongoCollection<FileModel>>(() =>
                dataBase.GetCollection<FileModel>(nameof(IStorage.Images)));
        }

        public Lazy<IMongoCollection<FileModel>> Images { get; }
    }
}