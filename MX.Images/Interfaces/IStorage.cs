using System;
using MongoDB.Driver;
using MX.Images.Models.Mongo;

namespace MX.Images.Interfaces
{
    public interface IStorage
    {
        Lazy<IMongoCollection<FileModel>> Images { get; }
    }
}