using System.Collections.ObjectModel;

namespace MX.Images
{
    public interface IFileScan
    {
        ReadOnlyCollection<FileModel> Handle(string file);
    }
}