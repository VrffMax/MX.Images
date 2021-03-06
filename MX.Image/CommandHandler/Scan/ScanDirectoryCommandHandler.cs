using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Command.Scan;
using MX.Images.ContainerInterface;
using MX.Images.Model.CQRS;

namespace MX.Images.CommandHandler.Scan
{
    public class ScanDirectoryCommandHandler
        : IRequestHandler<ScanDirectoryCommand, DirectoryModel>
    {
        private readonly IOptions _options;

        public ScanDirectoryCommandHandler(IOptions options)
        {
            _options = options;
        }

        public async Task<DirectoryModel> Handle(ScanDirectoryCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var directoriesTask = Task.Run(() =>
                    Directory.GetDirectories(request.SourcePath), cancellationToken);

                var fileTasks = _options.SearchPatterns.Select(searchPattern =>
                    Task.Run(() => Directory.GetFiles(request.SourcePath, searchPattern), cancellationToken)).ToArray();

                await Task.WhenAll(
                    directoriesTask,
                    Task.WhenAll(fileTasks));

                return new DirectoryModel(
                    Array.AsReadOnly(await directoriesTask),
                    Array.AsReadOnly(fileTasks.SelectMany(fileTask => fileTask.Result).ToArray()));
            }
            catch (UnauthorizedAccessException)
            {
                return new DirectoryModel();
            }
        }
    }
}