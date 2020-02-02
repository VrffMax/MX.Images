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

            var a = fileIsHashEqualTasks
                .Where(fileIsHashEqualTask => fileIsHashEqualTask.Result.IsHashEqual)
                .Select(fileIsHashEqualTask => Process(fileIsHashEqualTask.Result.File))
                .ToArray();

            return Unit.Value;
        }

        private static Task<(FileModel File, bool IsHashEqual)> IsFileCopyHashEqualAsync(FileModel file)
        {
            using var md5 = MD5.Create();
            using var fileStream = new FileStream(file.CopyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((file, md5.ComputeHash(fileStream).SequenceEqual(file.Hash)));
        }

        private async Task Process(FileModel file)
        {
            var filter = Builders<FileModel>.Filter.Eq(fileModel => fileModel.Id, file.Id);
            await _storage.Images.Value.DeleteOneAsync(filter);
        }
    }
}