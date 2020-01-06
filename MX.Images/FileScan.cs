using System;
using System.Collections.ObjectModel;
using System.Linq;
using MetadataExtractor;

namespace MX.Images
{
    public class FileScan
        : IFileScan
    {
        public ReadOnlyCollection<FileModel> Handle(string file) =>
            Array.AsReadOnly(
                ImageMetadataReader.ReadMetadata(file)
                    .SelectMany(directory => directory.Tags)
                    .Select(tag => new FileModel
                    {
                        DirectoryName = tag.DirectoryName,
                        Name = tag.Name,
                        Description = tag.Description,
                        Type = tag.Type
                    }).ToArray());
    }
}