using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MetadataExtractor;
using MX.Images.Commands;
using MX.Images.Containers;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class FileScanCommandHandler
        : IRequestHandler<FileScanCommand, FileModel>
    {
        private readonly IOptions _options;

        public FileScanCommandHandler(IOptions options) =>
            _options = options;

        public Task<FileModel> Handle(FileScanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(new FileModel
                {
                    Machine = _options.Machine,
                    Path = Path.GetDirectoryName(request.File),
                    Name = Path.GetFileName(request.File),
                    Tags = Array.AsReadOnly(
                        ImageMetadataReader.ReadMetadata(request.File)
                            .SelectMany(directory => directory.Tags)
                            .Select(tag => new FileModelTag
                            {
                                Directory = tag.DirectoryName,
                                Name = tag.Name,
                                Description = tag.Description,
                                Type = tag.Type
                            }).ToArray())
                });
            }
            catch (ImageProcessingException)
            {
                Console.WriteLine($"\t{Path.GetFileName(request.File)} - {nameof(ImageProcessingException)}");
                return Task.FromResult<FileModel>(default);
            }
        }
    }
}