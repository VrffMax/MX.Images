using System;
using System.Collections.ObjectModel;
using System.Linq;
using MetadataExtractor;

namespace MX.Images
{
    public class FileScan
        : IFileScan
    {
        public FileModel Handle(string file) => new FileModel
        {
            MachineName = Environment.MachineName,
            File = file,
            Tags = Array.AsReadOnly(
                ImageMetadataReader.ReadMetadata(file)
                    .SelectMany(directory => directory.Tags)
                    .Select(tag => new FileModelTag
                    {
                        DirectoryName = tag.DirectoryName,
                        Name = tag.Name,
                        Description = tag.Description,
                        Type = tag.Type
                    }).ToArray())
        };
    }
}