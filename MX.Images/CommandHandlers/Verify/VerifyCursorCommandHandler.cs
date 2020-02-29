using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands.Verify;
using MX.Images.Models;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Verify
{
    public class VerifyCursorCommandHandler
        : IRequestHandler<VerifyCursorCommand, Unit>
    {
        private readonly IState _state;

        public VerifyCursorCommandHandler(IState state)
        {
            _state = state;
        }

        public async Task<Unit> Handle(VerifyCursorCommand request, CancellationToken cancellationToken)
        {
            var fileIsHashEqualTasks = request.Files.Select(IsFileCopyHashEqualAsync).ToArray();

            await Task.WhenAll(fileIsHashEqualTasks);

            var verifyFailedImages = fileIsHashEqualTasks
                .Where(fileIsHashEqualTask => !fileIsHashEqualTask.Result.IsHashEqual)
                .Select(fileIsHashEqualTask => fileIsHashEqualTask.Result.File)
                .ToArray();

            if (verifyFailedImages.Any())
            {
                foreach (var verifyFailedImage in verifyFailedImages)
                    _state.Log(nameof(VerifyCursorCommandHandler), verifyFailedImage.CopyPath);

                var recopyFailedImageTasks = verifyFailedImages.Select(fileModel => new
                    {
                        SourceFile = Path.Combine(fileModel.Path, fileModel.Name),
                        DestinationFile = fileModel.CopyPath
                    })
                    .Select(context => FileCopy(context.SourceFile, context.DestinationFile))
                    .ToArray();

                await Task.WhenAll(recopyFailedImageTasks);
            }

            return Unit.Value;
        }

        private static Task<(FileModel File, bool IsHashEqual)> IsFileCopyHashEqualAsync(FileModel file)
        {
            using var md5 = MD5.Create();
            using var fileStream = new FileStream(file.CopyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((file, md5.ComputeHash(fileStream).SequenceEqual(file.Hash)));
        }

        private Task FileCopy(string sourceFile, string destinationFile)
        {
            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (Exception exception)
            {
                _state.Log(nameof(VerifyCursorCommandHandler), exception.Message);
            }

            return Task.CompletedTask;
        }
    }
}