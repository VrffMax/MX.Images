using System;
using System.Collections.ObjectModel;

namespace MX.Images
{
    public class Options
        : IOptions
    {
        public Options() =>
            SearchPatterns = Array.AsReadOnly(new[]
            {
                "*.jpg",
                "*.jpeg",
                "*.png",
                "*.gif"
            });

        public ReadOnlyCollection<string> SearchPatterns { get; }
    }
}