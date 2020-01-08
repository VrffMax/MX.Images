using System.Collections.ObjectModel;

namespace MX.Images
{
    public interface IOptions
    {
        string Machine { get; }

        string MongoConnectionString { get; }

        ReadOnlyCollection<string> SearchPatterns { get; }
    }
}