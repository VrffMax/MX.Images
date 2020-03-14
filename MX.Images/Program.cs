using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MX.Images.Commands.Exclude;
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
        private const string ResultPrefix = "*** Result ***";

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
            {CommandEnum.VerifyExclude, $"{MxImage} {CommandEnum.VerifyExclude} {SourcePath} {DestinationPath}"}
        };

        private static readonly IMediator Mediator;
        private static readonly IState State;

        static Program()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Options()).As<IOptions>();
            builder.RegisterInstance(new State()).As<IState>();
            builder.RegisterType<Storage>().As<IStorage>();
            builder.AddMediatR(typeof(Program).Assembly);

            var container = builder.Build();

            Mediator = container.Resolve<IMediator>();
            State = container.Resolve<IState>();
        }

        private static async Task Main(string[] args)
        {
            var ticks = Environment.TickCount64;
            {
                await Handle(args);

                Console.WriteLine(ResultPrefix);
                foreach (var message in State.Messages) Console.WriteLine(message);
            }
            Console.WriteLine($"Ticks\t{Environment.TickCount64 - ticks}");
        }

        private static Task Handle(string[] args)
        {
            return new CommandLine(args) switch
            {
                var (command, sourcePath)
                when command == CommandEnum.Scan && sourcePath.IsPath() =>
                Mediator.Send(new ScanCommand(sourcePath)),

                var (command, sourcePath, destinationPath)
                when command == CommandEnum.Sync && sourcePath.IsPath() && destinationPath.IsPath() =>
                Mediator.Send(new SyncCommand(sourcePath, destinationPath)),

                var (command, sourcePath, destinationPath)
                when command == CommandEnum.ScanSync && sourcePath.IsPath() && destinationPath.IsPath() =>
                ScanSync(Mediator, sourcePath, destinationPath),

                var (command, sourcePath)
                when command == CommandEnum.Verify && sourcePath.IsPath() =>
                Mediator.Send(new VerifyCommand(sourcePath)),

                var (command, sourcePath, destinationPath)
                when command == CommandEnum.VerifyExclude && sourcePath.IsPath() && destinationPath.IsPath() =>
                VerifyExclude(Mediator, sourcePath, destinationPath),

                var (command, sourcePath, destinationPath)
                when command == CommandEnum.Exclude && sourcePath.IsPath() && destinationPath.IsPath() =>
                Mediator.Send(new ExcludeCommand(sourcePath, destinationPath)),

                var (command)
                when command != CommandEnum.Help => command.Help(),

                _ => Help()
            };
        }

        private static async Task<Unit> ScanSync(IMediator mediator, string sourcePath, string destinationPath)
        {
            await mediator.Send(new ScanCommand(sourcePath));
            await mediator.Send(new SyncCommand(sourcePath, destinationPath));
            return Unit.Value;
        }

        private static async Task VerifyExclude(IMediator mediator, string sourcePath, string destinationPath)
        {
            await mediator.Send(new VerifyCommand(sourcePath));

            if (State.Messages.Any())
            {
                Console.WriteLine("No exclude");
                return;
            }

            await mediator.Send(new ExcludeCommand(sourcePath, destinationPath));
        }

        private static Task<Unit> Help()
        {
            Console.WriteLine(string.Join(Environment.NewLine, CommandHelp.Values.Select(help => help)));
            return Task.FromResult(Unit.Value);
        }

        private static Task<Unit> Help(this CommandEnum command)
        {
            Console.WriteLine(CommandHelp[command]);
            return Task.FromResult(Unit.Value);
        }

        private static bool IsPath(this string path)
        {
            try
            {
                var _ = new FileInfo(path);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private enum CommandEnum
        {
            Help,
            Scan,
            Sync,
            ScanSync,
            Verify,
            VerifyExclude,
            Exclude
        }

        private class CommandLine
        {
            private readonly CommandEnum _command = CommandEnum.Help;
            private readonly string _destinationPath;

            private readonly string _sourcePath;

            public CommandLine(string[] args)
            {
                var argsQueue = new Queue<string>(args);

                if (argsQueue.Any() && Enum.TryParse<CommandEnum>(argsQueue.Dequeue(), true, out var command))
                    _command = command;

                if (argsQueue.Any()) _sourcePath = argsQueue.Dequeue();

                if (argsQueue.Any()) _destinationPath = argsQueue.Dequeue();
            }

            public void Deconstruct(out CommandEnum command)
            {
                command = _command;
            }

            public void Deconstruct(out CommandEnum command, out string sourcePath)
            {
                command = _command;
                sourcePath = _sourcePath;
            }

            public void Deconstruct(out CommandEnum command, out string sourcePath, out string destinationPath)
            {
                command = _command;
                sourcePath = _sourcePath;
                destinationPath = _destinationPath;
            }
        }
    }
}