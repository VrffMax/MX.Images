using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Activators.Reflection;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MongoDB.Driver;
using MX.Images.Commands.Scan;
using MX.Images.Commands.Sync;
using MX.Images.Commands.Verify;
using MX.Images.Containers;
using MX.Images.Interfaces;
using MX.Images.Models;

namespace MX.Images
{
    public static class Program
    {
        private const string MxImage = "MX.Image";
        private const string SourcePath = "[Source path]";
        private const string DestinationPath = "[Destination path]";

        private static readonly Dictionary<CommandEnum, string> CommandHelp = new Dictionary<CommandEnum, string>
        {
            {CommandEnum.Help, $"{MxImage} {CommandEnum.Help}"},
            {CommandEnum.Scan, $"{MxImage} {CommandEnum.Scan} {SourcePath}"},
            {CommandEnum.Sync, $"{MxImage} {CommandEnum.Sync} {SourcePath} {DestinationPath}"},
            {CommandEnum.ScanSync, $"{MxImage} {CommandEnum.ScanSync} {SourcePath} {DestinationPath}"},
            {CommandEnum.Verify, $"{MxImage} {CommandEnum.Verify} {SourcePath}"}
        };

        private static readonly IMediator Mediator;

        static Program()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Options()).As<IOptions>();
            builder.RegisterInstance(new State()).As<IState>();
            builder.RegisterType<Storage>().As<IStorage>();
            builder.AddMediatR(typeof(Program).Assembly);

            var container = builder.Build();

            Mediator = container.Resolve<IMediator>();
        }

        private static async Task Main(string[] args)
        {
            var argsQueue = new Queue<string>(args);

            if (argsQueue.Count == 0 || !Enum.TryParse<CommandEnum>(argsQueue.Dequeue(), true, out var command))
            {
                await CommandEnum.Help.Help();
                return;
            }

            Console.WriteLine(command);
            Console.WriteLine();

            await command.Handle(argsQueue);

            Console.WriteLine("Done");
        }

        private static Task<Unit> Help()
        {
            Console.WriteLine(string.Join(Environment.NewLine, CommandHelp.Values.Select(help => help)));
            return Task.FromResult(Unit.Value);
        }

        private static Task<Unit> One(CommandEnum command, string sourcePath, Func<Task<Unit>> commandFunc) =>
            command switch
            {
                CommandEnum.Scan => Mediator.Send(new ScanCommand(sourcePath)),
                CommandEnum.Verify => Mediator.Send(new VerifyCommand(sourcePath)),
                _ => commandFunc()
            };

        private static Task<Unit> Two(CommandEnum command, string sourcePath, string destinationPath,
            Func<Task<Unit>> commandFunc) =>
            command switch
            {
                CommandEnum.Sync => Mediator.Send(new SyncCommand(sourcePath, destinationPath)),
                CommandEnum.ScanSync => ScanSync(Mediator, sourcePath, destinationPath),
                _ => commandFunc()
            };

        private static async Task<Unit> ScanSync(IMediator mediator, string sourcePath, string destinationPath)
        {
            await mediator.Send(new ScanCommand(sourcePath));
            await mediator.Send(new SyncCommand(sourcePath, destinationPath));
            return Unit.Value;
        }

        private enum CommandEnum
        {
            Help,
            Scan,
            Sync,
            ScanSync,
            Verify
        }

        private static Task<Unit> Help(this CommandEnum command)
        {
            Console.WriteLine(CommandHelp[command]);
            return Task.FromResult(Unit.Value);
        }

        private static Task<Unit> Handle(this CommandEnum command, Queue<string> argsQueue)
        {
            if (argsQueue.TryDequeue(out var sourcePath))
            {
                return One(command, sourcePath, () =>
                {
                    if (argsQueue.TryDequeue(out var destinationPath))
                    {
                        return Two(command, sourcePath, destinationPath, () => command.Help());
                    }

                    return command.Help();
                });
            }

            return Help();
        }
    }
}