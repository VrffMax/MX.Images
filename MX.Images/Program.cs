﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MX.Images.Commands.Scan;
using MX.Images.Commands.Sync;
using MX.Images.Commands.Verify;
using MX.Images.Containers;
using MX.Images.Interfaces;

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
            {CommandEnum.Help, $"{MxImage} {CommandEnum.Help}"},
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
                Console.WriteLine(CommandHelp[CommandEnum.Help]);
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

                    await (await GetMediatorAsync()).Send(new ScanCommand(queue.Dequeue()));
                    break;

                case CommandEnum.Sync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.Sync]);
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new SyncCommand(queue.Dequeue(), queue.Dequeue()));
                    break;

                case CommandEnum.ScanSync:
                    if (queue.Count != 2)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.ScanSync]);
                        return;
                    }

                    await ScanSync(queue.Dequeue(), queue.Dequeue());
                    break;

                case CommandEnum.Verify:
                    if (queue.Count != 1)
                    {
                        Console.WriteLine(CommandHelp[CommandEnum.Verify]);
                        return;
                    }

                    await (await GetMediatorAsync()).Send(new VerifyCommand(queue.Dequeue()));
                    break;
            }

            Console.WriteLine("Done");
        }

        private static async Task ScanSync(string sourcePath, string destinationPath)
        {
            var mediator = await GetMediatorAsync();

            await mediator.Send(new ScanCommand(sourcePath));
            await mediator.Send(new SyncCommand(sourcePath, destinationPath));
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