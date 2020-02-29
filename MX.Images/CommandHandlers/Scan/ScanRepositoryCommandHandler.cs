using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Scan;
using MX.Images.Interfaces;
using MX.Images.Models;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Scan
{
    public class ScanRepositoryCommandHandler
        : IRequestHandler<ScanRepositoryCommand>
    {
        private readonly IState _state;
        private readonly IStorage _storageDelete;
        private readonly IStorage _storageFindInsert;


        public ScanRepositoryCommandHandler(
            IStorage storageFindInsert,
            IStorage storageDelete,
            IState state)
        {
            _storageFindInsert = storageFindInsert;
            _storageDelete = storageDelete;
            _state = state;
        }

        public async Task<Unit> Handle(ScanRepositoryCommand request, CancellationToken cancellationToken)
        {
            if (!request.Files.Any()) return Unit.Value;

            var path = request.Files.Select(fileModel => fileModel.Path).First();
            var storageImages = await GetStorageImages(path, request.Files);

            await Task.WhenAll(
                InsertNewImages(path, request.Files, storageImages, cancellationToken),
                DeleteNotExistsImages(path, request.Files, storageImages, cancellationToken));

            return Unit.Value;
        }

        private async Task<ReadOnlyCollection<FileModel>> GetStorageImages(string path,
            ReadOnlyCollection<FileModel> files)
        {
            var machine = files.Select(fileModel => fileModel.Machine).First();

            var findFilter = Builders<FileModel>.Filter.Where(fileModel => true
                                                                           && fileModel.Path == path
                                                                           && fileModel.Machine == machine);

            var findOptions = new FindOptions<FileModel, FileModel>
            {
                Projection = Builders<FileModel>.Projection
                    .Include(fileModel => fileModel.Id)
                    .Include(fileModel => fileModel.Name)
            };

            try
            {
                var storageImages =
                    (await (await _storageFindInsert.Images.Value.FindAsync(findFilter, findOptions)).ToListAsync())
                    .AsReadOnly();

                return storageImages;
            }
            catch (Exception exception)
            {
                _state.Log(nameof(ScanRepositoryCommandHandler), exception.Message);

                return Array.AsReadOnly(new FileModel[] { });
            }
        }

        private async Task InsertNewImages(
            string path,
            ReadOnlyCollection<FileModel> fileModels,
            ReadOnlyCollection<FileModel> storageImages,
            CancellationToken cancellationToken)
        {
            var insertImages = fileModels
                .Where(fileModel => storageImages.All(storageImage => storageImage.Name != fileModel.Name))
                .ToArray();

            if (insertImages.Any())
                await _storageFindInsert.Images.Value.InsertManyAsync(insertImages,
                    cancellationToken: cancellationToken);
        }

        private async Task DeleteNotExistsImages(
            string path,
            ReadOnlyCollection<FileModel> fileModels,
            ReadOnlyCollection<FileModel> storageImages,
            CancellationToken cancellationToken)
        {
            var deleteImages = storageImages
                .Where(storageImage => fileModels.All(fileModel => fileModel.Name != storageImage.Name))
                .Select(fileModel => fileModel.Id)
                .ToArray();

            if (deleteImages.Any())
            {
                var deleteFilter = Builders<FileModel>.Filter.In(fileModel => fileModel.Id, deleteImages);
                await _storageDelete.Images.Value.DeleteManyAsync(deleteFilter, cancellationToken);
            }
        }
    }
}