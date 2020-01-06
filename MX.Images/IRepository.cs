using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MX.Images
{
    public interface IRepository
    {
        Task HandleAsync(ReadOnlyCollection<FileModel> fileModels);
    }
}