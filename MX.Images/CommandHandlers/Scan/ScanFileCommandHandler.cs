using MediatR;
using MetadataExtractor;
using MX.Images.Commands.Scan;
using MX.Images.Interfaces;
using MX.Images.Models;
using MX.Images.Models.Mongo;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MX.Images.CommandHandlers.Scan
{
    public class ScanFileCommandHandler
        : IRequestHandler<ScanFileCommand, FileModel>
    {
        private readonly IOptions _options;
        private readonly IState _state;

        public ScanFileCommandHandler(IOptions options, IState state)
        {
            _options = options;
            _state = state;
        }

        public Task<FileModel> Handle(ScanFileCommand request,
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
                var message = $@"*** Warning *** ""{Path.GetFileName(request.File)}"" & ""{nameof(ImageProcessingException)}""";

                Console.WriteLine(message);
                _state.Messages.Enqueue(message);

                return Task.FromResult<FileModel>(default);
            }
        }
    }
}