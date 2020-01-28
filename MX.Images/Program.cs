using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MX.Images.Commands.Refactor;
using MX.Images.Commands.Scan;
using MX.Images.Commands.Verify;
using MX.Images.Containers;
using MX.Images.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MX.Images
{
    public static class Program
    {
        private enum CommandEnum
        {
            Help,
            Scan,
            Sync,
            ScanSync,
            Verify
        }

        private static async Task Main(string[] args)
        {
            var queue = new Queue<string>(args);

            queue.Dequeue();
            if (queue.Count == 0 || !Enum.TryParse<CommandEnum>(queue.Dequeue(), true, out var command))
            {
                Console.WriteLine("MX.Images Help");
                return;
            }

            switch (command)
            {
                case CommandEnum.Help:
                    Console.WriteLine(@"MX.Images
	Scan [Source directory]
	Sync [Source directory] [Destination directory]
	ScanSync [Source directory] [Destination directory]
	Verify [Source directory]
");
                    return;

                case CommandEnum.Scan:
                    if (queue.Count != 1)
                    {
                        Console.WriteLine($"MX.Images {CommandEnum.Scan} [Source directory]");
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new RootScanCommand(queue.Dequeue()));
                    return;

                case CommandEnum.Sync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine($"MX.Images {CommandEnum.Sync} [Source directory] [Destination directory]");
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new RefactorCommand(queue.Dequeue(), queue.Dequeue()));
                    return;

                case CommandEnum.ScanSync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine(
                            $"MX.Images {CommandEnum.ScanSync} [Source directory] [Destination directory]");
                        return;
                    }

                    var mediator = await GetMediatorAsync();

                    await mediator.Send(new RootScanCommand(queue.Dequeue()));
                    await mediator.Send(new RefactorCommand(queue.Dequeue(), queue.Dequeue()));

                    return;

                case CommandEnum.Verify:
                    if (queue.Count != 1)
                    {
                        Console.WriteLine($"MX.Images {CommandEnum.Verify} [Source directory]");
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new VerifyCommand(queue.Dequeue()));
                    return;
            }
        }

        private static Task<IMediator> GetMediatorAsync()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Options()).As<IOptions>();
            builder.RegisterType<Storage>().As<IStorage>();
            builder.AddMediatR(typeof(Program).Assembly);

            return Task.FromResult(builder.Build().Resolve<IMediator>());
        }
    }
}