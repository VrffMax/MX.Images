using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Sync;
using MX.Images.Interfaces;
using MX.Images.Models;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Sync
{
    public class SyncCopyCommandHandler
        : IRequestHandler<SyncCopyCommand>
    {
        private readonly IStorage _storage;
        private readonly IState _state;

        public SyncCopyCommandHandler(
            IStorage storage,
            IState state)
        {
            _storage = storage;
            _state = state;
        }

        public async Task<Unit> Handle(SyncCopyCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Copy {request.RefactorDirectory.Path}");

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
                var destinationFile = Path.Combine(destinationPath, $"{GetStringHash(source.Path)}{extension}");

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

            if (source.LastWriteTimeUtc == lastWriteTimeUtc && File.Exists(destinationFile))
            {
                return Task.CompletedTask;
            }

            var filter = Builders<FileModel>.Filter.Where(fileModel => fileModel.Id == source.Id);
            var update = Builders<FileModel>.Update
                .Set(fileModel => fileModel.LastWriteTimeUtc, lastWriteTimeUtc)
                .Set(fileModel => fileModel.CopyPath, destinationFile)
                .Set(fileModel => fileModel.Hash, GetFileHash(sourceFile));

            return Task.WhenAll(
                _storage.Images.Value.UpdateOneAsync(filter, update, default, cancellationToken),
                this.FileCopy(sourceFile, destinationFile)
            );
        }

        private string GetStringHash(string value)
        {
            using var md5 = MD5.Create();
            return string.Concat(md5.ComputeHash(Encoding.ASCII.GetBytes(value))
                .Select(item => item.ToString("x2")));
        }

        private byte[] GetFileHash(string fileName)
        {
            using var md5 = MD5.Create();
            using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return md5.ComputeHash(fileStream);
        }

        private Task FileCopy(string sourceFile, string destinationFile)
        {
            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }

            catch (Exception exception)
            {
                var message = $"*** Error *** {exception.Message}";

                Console.WriteLine(message);
                _state.Errors.Enqueue(message);
            }

            return Task.CompletedTask;
        }
    }
}