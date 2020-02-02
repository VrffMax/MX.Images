using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Verify;
using MX.Images.Interfaces;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Verify
{
    public class VerifyCursorCommandHandler
        : IRequestHandler<VerifyCursorCommand, Unit>
    {
        private readonly IStorage _storage;

        public VerifyCursorCommandHandler(IStorage storage) =>
            _storage = storage;

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
                {
                    Console.WriteLine($@"*** Verify failed *** {verifyFailedImage.CopyPath}");
                }

                var deleteImages = verifyFailedImages
                    .Select(fileModel => fileModel.Id)
                    .ToArray();

                var deleteFilter = Builders<FileModel>.Filter.In(fileModel => fileModel.Id, deleteImages);
                await _storage.Images.Value.DeleteManyAsync(deleteFilter, cancellationToken);
            }

            return Unit.Value;
        }

        private static Task<(FileModel File, bool IsHashEqual)> IsFileCopyHashEqualAsync(FileModel file)
        {
            using var md5 = MD5.Create();
            using var fileStream = new FileStream(file.CopyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((file, md5.ComputeHash(fileStream).SequenceEqual(file.Hash)));
        }
    }
}