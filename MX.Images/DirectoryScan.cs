using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

                var fileTasks = _options.SearchPatterns.Select(searchPattern =>
                    Task.Run(() => Directory.GetFiles(path, searchPattern))).ToArray();

                await Task.WhenAll(
                    directoriesTask,
                    Task.WhenAll(fileTasks));

                var files = Array.AsReadOnly(fileTasks.SelectMany(fileTask =>
                    fileTask.Result).ToArray());

                return (directoriesTask.Result, files);
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