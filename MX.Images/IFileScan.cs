using System.Collections.ObjectModel;

namespace MX.Images
{
    public interface IFileScan
    {
        FileModel Handle(string file);
    }
}