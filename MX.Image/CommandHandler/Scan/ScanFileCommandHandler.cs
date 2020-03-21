using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MetadataExtractor;
using MX.Images.Command.Scan;
using MX.Images.ContainerInterface;
using MX.Images.Model;
using MX.Images.Model.Mongo;

namespace MX.Images.CommandHandler.Scan
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
                                Description = tag.Description?.Trim(),
                                Type = tag.Type
                            }).ToArray())
                });
            }
            catch (ImageProcessingException)
            {
                _state.Log(
                    nameof(ScanFileCommandHandler),
                    new ApplicationException(
                        $@"""{Path.GetDirectoryName(request.File)}"" & ""{Path.GetFileName(request.File)}"" & ""{nameof(ImageProcessingException)}"""));

                return Task.FromResult<FileModel>(default);
            }
        }
    }
}