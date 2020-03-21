using System;
using MongoDB.Driver;
using MX.Images.Model.Mongo;

namespace MX.Images.ContainerInterface
{
    public interface IStorage
    {
        Lazy<IMongoCollection<FileModel>> Images { get; }
    }
}