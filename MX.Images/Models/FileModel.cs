using System;
using System.Collections.ObjectModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MX.Images.Models
{
    public class FileModel
    {
        [BsonId] public ObjectId Id { get; set; }

        public string Machine { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public long Length { get; set; }

        public DateTime CreationTimeUtc { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public string CopyPath { get; set; }

        public byte[] Hash { get; set; }

        public ReadOnlyCollection<FileModelTag> Tags { get; set; }
    }
}