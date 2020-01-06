using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MX.Images
{
    public interface IDirectoryScan
    {
        Task<(ReadOnlyCollection<string> Directories, ReadOnlyCollection<string> Files)> HandleAsync(
            string path);
    }
}