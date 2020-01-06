using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MX.Images
{
    public class Repository
        : IRepository
    {
        public async Task HandleAsync(ReadOnlyCollection<FileModel> fileModels)
        {
            if (!fileModels.Any())
            {
                return;
            }

            var client = new MongoClient("mongodb://localhost:27017");

            var database = client.GetDatabase("MX");
            var images = database.GetCollection<FileModel>("Images");

            await images.InsertManyAsync(fileModels);
        }
    }
}