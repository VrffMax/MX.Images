using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MX.Images.Commands;
using MX.Images.Containers;
using MX.Images.Interfaces;

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
            builder.AddMediatR(typeof(Program).Assembly);

            _container = builder.Build();
        }

        private static Task Main(string[] args)
        {
            InitializeContainer();

            if (args.Length != 1)
            {
                Console.WriteLine("Root images directory for scan required");
                return Task.CompletedTask;
            }

            return SendRootScanCommand(args.First());
        }

        private static async Task SendRootScanCommand(string path)
        {
            var mediator = _container.Resolve<IMediator>();

            var tickCount = Environment.TickCount;

            // await mediator.Send(new RootScanCommand(path));
            await mediator.Send(new RefactorCommand(path));

            Console.WriteLine($"TickCount: {Environment.TickCount - tickCount}");
        }
    }
}