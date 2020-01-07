using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MX.Images
{
    public class Repository
        : IRepository
    {
        private readonly IStorage _storageFindInsert;
        private readonly IStorage _storageDelete;

        public Repository(
            IStorage storageFindInsert,
            IStorage storageDelete)
        {
            _storageFindInsert = storageFindInsert;
            _storageDelete = storageDelete;
        }

        public async Task HandleAsync(ReadOnlyCollection<FileModel> fileModels)
        {
            if (!fileModels.Any())
            {
                return;
            }

            var storageImages = await GetStorageImages(fileModels);

            await Task.WhenAll(
                InsertNewImages(fileModels, storageImages),
                DeleteNotExistsImages(fileModels, storageImages));
        }

        private async Task<ReadOnlyCollection<FileModel>> GetStorageImages(ReadOnlyCollection<FileModel> fileModels)
        {
            var machine = fileModels.Select(fileModel => fileModel.Machine).First();
            var path = fileModels.Select(fileModel => fileModel.Path).First();

            var findFilter = Builders<FileModel>.Filter.Where(fileModel => true
                                                                           && fileModel.Path == path
                                                                           && fileModel.Machine == machine);

            var findOptions = new FindOptions<FileModel, FileModel>
            {
                Projection = Builders<FileModel>.Projection
                    .Include(fileModel => fileModel.Id)
                    .Include(fileModel => fileModel.Name)
            };

            var storageImages =
                (await (await _storageFindInsert.Images.Value.FindAsync(findFilter, findOptions)).ToListAsync())
                .AsReadOnly();

            return storageImages;
        }

        private async Task InsertNewImages(
            ReadOnlyCollection<FileModel> fileModels,
            ReadOnlyCollection<FileModel> storageImages)
        {
            var insertImages = fileModels
                .Where(fileModel => storageImages.All(storageImage => storageImage.Name != fileModel.Name))
                .ToArray();

            if (insertImages.Any())
            {
                await _storageFindInsert.Images.Value.InsertManyAsync(insertImages);
            }
        }

        private async Task DeleteNotExistsImages(
            ReadOnlyCollection<FileModel> fileModels,
            ReadOnlyCollection<FileModel> storageImages)
        {
            var deleteImages = storageImages
                .Where(storageImage => fileModels.All(fileModel => fileModel.Name != storageImage.Name))
                .Select(fileModel => fileModel.Id)
                .ToArray();

            if (deleteImages.Any())
            {
                var deleteFilter = Builders<FileModel>.Filter.In(fileModel => fileModel.Id, deleteImages);
                await _storageDelete.Images.Value.DeleteManyAsync(deleteFilter);
            }
        }
    }
}