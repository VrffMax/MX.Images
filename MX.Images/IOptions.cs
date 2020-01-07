using System.Collections.ObjectModel;

namespace MX.Images
{
    public interface IOptions
    {
        ReadOnlyCollection<string> SearchPatterns { get; }
    }
}