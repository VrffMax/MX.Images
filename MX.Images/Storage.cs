using System;
using MongoDB.Driver;

namespace MX.Images
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