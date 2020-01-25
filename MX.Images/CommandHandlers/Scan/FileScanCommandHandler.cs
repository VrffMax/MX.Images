using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MetadataExtractor;
using MX.Images.Commands;
using MX.Images.Commands.Scan;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images.CommandHandlers.Scan
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
                var fileInfo = new FileInfo(request.File);

                return Task.FromResult(new FileModel
                {
                    Machine = _options.Machine,

                    Path = Path.GetDirectoryName(request.File),
                    Name = Path.GetFileName(request.File),

                    Length = fileInfo.Length,
                    CreationTimeUtc = fileInfo.CreationTimeUtc,
                    LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,

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
                Console.WriteLine(
                    $@"*** Warning *** ""{Path.GetFileName(request.File)}"" & ""{nameof(ImageProcessingException)}""");

                return Task.FromResult<FileModel>(default);
            }
        }
    }
}