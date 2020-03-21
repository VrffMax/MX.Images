using System;
using MongoDB.Driver;
using MX.Images.ContainerInterface;
using MX.Images.Model.Mongo;

namespace MX.Images.Container
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