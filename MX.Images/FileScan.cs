using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MetadataExtractor;

namespace MX.Images
{
    public class FileScan
        : IFileScan
    {
        public FileModel Handle(string file) => new FileModel
        {
            Machine = Environment.MachineName,
            Path = Path.GetDirectoryName(file),
            Name = Path.GetFileName(file),
            Tags = Array.AsReadOnly(
                ImageMetadataReader.ReadMetadata(file)
                    .SelectMany(directory => directory.Tags)
                    .Select(tag => new FileModelTag
                    {
                        Directory = tag.DirectoryName,
                        Name = tag.Name,
                        Description = tag.Description,
                        Type = tag.Type
                    }).ToArray())
        };
    }
}