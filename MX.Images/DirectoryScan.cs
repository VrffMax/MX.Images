using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace MX.Images
{
    public class DirectoryScan
        : IDirectoryScan
    {
        private readonly IOptions _options;

        public DirectoryScan(IOptions options) =>
            _options = options;

        public async Task<(ReadOnlyCollection<string> Directories, ReadOnlyCollection<string> Files)> HandleAsync(
            string path)
        {
            try
            {
                var directoriesTask = Task.Run(() =>
                    Array.AsReadOnly(Directory.GetDirectories(path)));

                var filesTask = Task.Run(() =>
                    Array.AsReadOnly(Directory.GetFiles(path, _options.SearchPattern)));

                await Task.WhenAll(directoriesTask, filesTask)
                    .ConfigureAwait(false);

                return (directoriesTask.Result, filesTask.Result);
            }
            catch (UnauthorizedAccessException)
            {
                return (
                    Array.AsReadOnly(new string[0]),
                    Array.AsReadOnly(new string[0]));
            }
        }
    }
}