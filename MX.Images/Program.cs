using System;
using System.Collections;
using System.Collections.Generic;
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

            if (args.Length != 2)
            {
                Console.WriteLine("Root images directory for scan required");
                return Task.CompletedTask;
            }

            var queue = new Queue<string>(args);

            return SendRootScanCommand(queue.Dequeue(), queue.Dequeue());
        }

        private static async Task SendRootScanCommand(string fromPath, string toPath)
        {
            var mediator = _container.Resolve<IMediator>();

            var tickCount = Environment.TickCount;

            await mediator.Send(new RootScanCommand(fromPath));
            await mediator.Send(new RefactorCommand(toPath));

            Console.WriteLine($"TickCount: {Environment.TickCount - tickCount}");
        }
    }
}