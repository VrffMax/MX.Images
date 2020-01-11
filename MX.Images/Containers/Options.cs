using System;
using System.Collections.ObjectModel;
using MX.Images.Interfaces;

namespace MX.Images.Containers
{
    public class Options
        : IOptions
    {
        public Options()
        {
            Machine = Environment.MachineName;

            MongoConnectionString = "mongodb://localhost:27017";

            SearchPatterns = Array.AsReadOnly(new[]
            {
                "*.jpg",
                "*.jpeg",
                "*.png",
                "*.gif"
            });
        }

        public string Machine { get; }

        public string MongoConnectionString { get; }

        public ReadOnlyCollection<string> SearchPatterns { get; }
    }
}