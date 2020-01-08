using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace MX.Images
{
    public static class Program
    {
        private static IContainer _container;

        private static void InitializeContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Options()).As<IOptions>();

            builder.RegisterType<Storage>().As<IStorage>();
            builder.RegisterType<FileScan>().As<IFileScan>();
            builder.RegisterType<DirectoryScan>().As<IDirectoryScan>();
            builder.RegisterType<RootScan>().As<IRootScan>();
            builder.RegisterType<Repository>().As<IRepository>();

            _container = builder.Build();
        }

        private static void Main(string[] args)
        {
            InitializeContainer();

            if (args.Length != 1)
            {
                Console.WriteLine("Root images directory for scan required");
                return;
            }

            var tickCount = Environment.TickCount;

            Task.Run(() => _container.Resolve<IRootScan>().HandleAsync(_container, args.First()))
                .GetAwaiter()
                .GetResult();

            Console.WriteLine($"TickCount: {Environment.TickCount - tickCount}");
        }
    }
}