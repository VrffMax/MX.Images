using System.Collections.ObjectModel;

namespace MX.Images.Interfaces
{
    public interface IOptions
    {
        string Machine { get; }

        string MongoConnectionString { get; }

        ReadOnlyCollection<string> SearchPatterns { get; }
    }
}