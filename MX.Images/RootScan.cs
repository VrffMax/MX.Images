using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace MX.Images
{
    public class RootScan
        : IRootScan
    {
        public async Task HandleAsync(IContainer container, string path)
        {
            Console.WriteLine(path);

            var scan = await container.Resolve<IDirectoryScan>()
                .HandleAsync(path);

            var rootScanTasks = scan.Directories
                .Select(directory => container.Resolve<IRootScan>().HandleAsync(container, directory))
                .ToArray();

            var fileTasks = scan.Files
                .Select(file => Task.Run(() => container.Resolve<IFileScan>().Handle(file)))
                .ToArray();

            await Task.WhenAll(
                Task.WhenAll(rootScanTasks),
                Task.WhenAll(fileTasks));

            await container.Resolve<IRepository>()
                .HandleAsync(Array.AsReadOnly(fileTasks.Select(fileTask => fileTask.Result).ToArray()));
        }
    }
}