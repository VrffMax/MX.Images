using System;
using System.IO;
using System.Linq;
using MetadataExtractor;

namespace MX.Images
{
    public class FileScan
        : IFileScan
    {
        private readonly IOptions _options;

        public FileScan(IOptions options) =>
            _options = options;

        public FileModel Handle(string file) => new FileModel
        {
            Machine = _options.Machine,
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