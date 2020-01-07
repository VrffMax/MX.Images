using System;
using MongoDB.Driver;

namespace MX.Images
{
    public interface IStorage
    {
        Lazy<IMongoCollection<FileModel>> Images { get; }
    }
}