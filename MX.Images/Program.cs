using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
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

        private static readonly Dictionary<CommandEnum, (string Help, int Parameters)> _commandHelp = new Dictionary<CommandEnum, (string, int)>
        {
            {CommandEnum.Help, ($"{MxImage} {CommandEnum.Help}", 0)},
            {CommandEnum.Scan, ($"{MxImage} {CommandEnum.Scan} {SourcePath}", 1)},
            {CommandEnum.Sync, ($"{MxImage} {CommandEnum.Sync} {SourcePath} {DestinationPath}", 2)},
            {CommandEnum.ScanSync, ($"{MxImage} {CommandEnum.ScanSync} {SourcePath} {DestinationPath}", 2)},
            {CommandEnum.Verify, ($"{MxImage} {CommandEnum.Verify} {SourcePath}", 1)}
        };

        private static readonly IMediator _mediator;
        private static readonly IState _state;

        static Program()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Options()).As<IOptions>();
            builder.RegisterInstance(new State()).As<IState>();
            builder.RegisterType<Storage>().As<IStorage>();
            builder.AddMediatR(typeof(Program).Assembly);

            var container = builder.Build();

            _mediator = container.Resolve<IMediator>();
            _state = container.Resolve<IState>();
        }

        private static async Task Main(string[] args)
        {
            var queue = new Queue<string>(args);

            if (queue.Count == 0 || !Enum.TryParse<CommandEnum>(queue.Dequeue(), true, out var command))
            {
                Console.WriteLine(_commandHelp[CommandEnum.Help]);
                return;
            }

            Console.WriteLine(command);
            Console.WriteLine();

            var commandHelp = _commandHelp[command];

            if (queue.Count != commandHelp.Parameters)
            {
                Console.WriteLine(commandHelp.Help);
                return;
            }

            switch (command)
            {
                case CommandEnum.Help:
                    Console.WriteLine(string.Join(Environment.NewLine, _commandHelp.Values.Select(value => value.Help)));
                    return;

                case CommandEnum.Scan:
                    await _mediator.Send(new ScanCommand(queue.Dequeue()));
                    break;

                case CommandEnum.Sync:
                    await _mediator.Send(new SyncCommand(queue.Dequeue(), queue.Dequeue()));
                    break;

                case CommandEnum.ScanSync:
                    await ScanSync(_mediator, queue.Dequeue(), queue.Dequeue());
                    break;

                case CommandEnum.Verify:
                    await _mediator.Send(new VerifyCommand(queue.Dequeue()));
                    break;
            }

            Console.WriteLine("Done");
        }

        private static async Task ScanSync(IMediator mediator, string sourcePath, string destinationPath)
        {
            await mediator.Send(new ScanCommand(sourcePath));
            await mediator.Send(new SyncCommand(sourcePath, destinationPath));
        }
    }
}