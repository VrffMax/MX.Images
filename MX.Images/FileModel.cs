using System.Collections.ObjectModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MX.Images
{
    public class FileModel
    {
        [BsonId] public ObjectId Id { get; set; }

        public string MachineName { get; set; }

        public string File { get; set; }

        public ReadOnlyCollection<FileModelTag> Tags { get; set; }
    }
}