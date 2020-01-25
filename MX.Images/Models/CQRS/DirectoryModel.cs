using System;
using System.Collections.ObjectModel;

namespace MX.Images.Models.CQRS
{
    public class DirectoryModel
    {
        public DirectoryModel() =>
            Directories = Files = Array.AsReadOnly(new string[0]);

        public DirectoryModel(
            ReadOnlyCollection<string> directories,
            ReadOnlyCollection<string> files)
        {
            Directories = directories;
            Files = files;
        }

        public ReadOnlyCollection<string> Directories { get; }

        public ReadOnlyCollection<string> Files { get; }
    }
}