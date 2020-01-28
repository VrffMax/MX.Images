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
using System.Collections.ObjectModel;
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

        private const string MxImage = "MX.Image";
        private const string SourcePath = "[Source path]";
        private const string DestinationPath = "[Destination path]";

        private static readonly Dictionary<CommandEnum, string> CommandHelp = new Dictionary<CommandEnum, string>
        {
            {CommandEnum.Scan, $"{MxImage} {CommandEnum.Scan} {SourcePath}"},
            {CommandEnum.Sync, $"{MxImage} {CommandEnum.Sync} {SourcePath} {DestinationPath}"},
            {CommandEnum.ScanSync, $"{MxImage} {CommandEnum.ScanSync} {SourcePath} {DestinationPath}"},
            {CommandEnum.Verify, $"{MxImage} {CommandEnum.Verify} {SourcePath}"},
        };

        private static async Task Main(string[] args)
        {
            var queue = new Queue<string>(args);

            if (queue.Count == 0 || !Enum.TryParse<CommandEnum>(queue.Dequeue(), true, out var command))
            {
                Console.WriteLine("MX.Images Help");
                return;
            }

            Console.WriteLine(command);

            switch (command)
            {
                case CommandEnum.Help:
                    Console.WriteLine(string.Join(Environment.NewLine, CommandHelp.Values));
                    return;

                case CommandEnum.Scan:
                    if (queue.Count != 1)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.Scan]);
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new RootScanCommand(queue.Dequeue()));
                    return;

                case CommandEnum.Sync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.Sync]);
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new RefactorCommand(queue.Dequeue(), queue.Dequeue()));
                    return;

                case CommandEnum.ScanSync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.ScanSync]);
                        return;
                    }

                    await ScanSync(queue.Dequeue(), queue.Dequeue());
                    return;

                case CommandEnum.Verify:
                    if (queue.Count != 1)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.Verify]);
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

        private static async Task ScanSync(string sourcePath, string destinationPath)
        {
            var mediator = await GetMediatorAsync();

            await mediator.Send(new RootScanCommand(sourcePath));
            await mediator.Send(new RefactorCommand(sourcePath, destinationPath));
        }
    }
}