using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands;
using MX.Images.Containers;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class DirectoryScanCommandHandler
        : IRequestHandler<DirectoryScanCommand, DirectoryModel>
    {
        private readonly IOptions _options;

        public DirectoryScanCommandHandler(IOptions options) =>
            _options = options;

        public async Task<DirectoryModel> Handle(DirectoryScanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var directoriesTask = Task.Run(() =>
                    Array.AsReadOnly(Directory.GetDirectories(request.SourcePath)), cancellationToken);

                var fileTasks = _options.SearchPatterns.Select(searchPattern =>
                    Task.Run(() => Directory.GetFiles(request.SourcePath, searchPattern), cancellationToken)).ToArray();

                await Task.WhenAll(
                    directoriesTask,
                    Task.WhenAll(fileTasks));

                var files = Array.AsReadOnly(fileTasks.SelectMany(fileTask =>
                    fileTask.Result).ToArray());

                return new DirectoryModel(await directoriesTask, files);
            }
            catch (UnauthorizedAccessException)
            {
                return new DirectoryModel();
            }
        }
    }
}