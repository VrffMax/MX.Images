using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class RefactorCopyCommandHandler
        : IRequestHandler<RefactorCopyCommand>
    {
        private readonly IStorage _storage;

        public RefactorCopyCommandHandler(IStorage storage) =>
            _storage = storage;

        public async Task<Unit> Handle(RefactorCopyCommand request, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(request.RefactorDirectory.Path);

            var filesTasks = request.RefactorDirectory.Files.Select(file =>
            {
                var destinationPath = Path.Combine(request.RefactorDirectory.Path, file.Name);

                return file.Sources.Count() == 1
                    ? SyncOneFileAsync(file, destinationPath, cancellationToken)
                    : SyncManyFileAsync(file, destinationPath, cancellationToken);
            }).ToArray();

            await Task.WhenAll(filesTasks);

            return Unit.Value;
        }

        private Task SyncOneFileAsync(
            RefactorFileModel file,
            string destinationFile,
            CancellationToken cancellationToken)
        {
            if (Directory.Exists(destinationFile))
            {
                Directory.Delete(destinationFile, true);
            }

            return CopyFileAsync(file.Sources.Single(), destinationFile, cancellationToken);
        }

        private Task SyncManyFileAsync(
            RefactorFileModel file,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            Directory.CreateDirectory(destinationPath);

            var extension = Path.GetExtension(file.Name);

            var sourcesTasks = file.Sources.Select(source =>
            {
                var destinationFile = Path.Combine(destinationPath, $"{GetHashMd5(source.Path)}{extension}");

                return CopyFileAsync(source, destinationFile, cancellationToken);
            }).ToArray();

            return Task.WhenAll(sourcesTasks);
        }

        private Task CopyFileAsync(
            FileModel source,
            string destinationFile,
            CancellationToken cancellationToken)
        {
            var sourceFile = Path.Combine(source.Path, source.Name);
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(sourceFile);

            if (source.LastWriteTimeUtc == lastWriteTimeUtc)
            {
                return File.Exists(destinationFile)
                    ? Task.CompletedTask
                    : Task.Run(() => File.Copy(sourceFile, destinationFile), cancellationToken);
            }

            var filter = Builders<FileModel>.Filter.Where(fileModel => fileModel.Id == source.Id);
            var update = Builders<FileModel>.Update.Set(fileModel => fileModel.LastWriteTimeUtc, lastWriteTimeUtc);

            return Task.WhenAll(
                _storage.Images.Value.UpdateOneAsync(filter, update, default, cancellationToken),
                Task.Run(() => File.Copy(sourceFile, destinationFile, true), cancellationToken)
            );
        }

        private string GetHashMd5(string value)
        {
            using var md5 = MD5.Create();
            return string.Concat(md5.ComputeHash(Encoding.ASCII.GetBytes(value))
                .Select(item => item.ToString("x2")));
        }
    }
}