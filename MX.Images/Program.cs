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
using System.Linq;
using System.Threading.Tasks;

namespace MX.Images
{
	public static class Program
	{
		private enum CommandEnum
		{
			Scan,
			Sync,
			ScanSync,
			Verify
		}

		private static async Task Main(string[] args)
		{
			if (args.Length < 2 || args.Length > 3)
			{
				Console.WriteLine("MX.Images [Scan | Sync | ScanSync | Verify] [Source directory] [Destination directory]");
				return;
			}

			var queue = new Queue<string>(args);

			if (!Enum.TryParse<CommandEnum>(queue.Dequeue(), true, out var command))
			{
				Console.WriteLine($"Commands: {CommandEnum.Scan}, {CommandEnum.Sync}, {CommandEnum.ScanSync} or {CommandEnum.Verify}");
				return;
			}

			var isSync = new[] { CommandEnum.Sync, CommandEnum.ScanSync }.Contains(command);
			if (isSync && args.Length != 3)
			{
				Console.WriteLine("MX.Images [Sync | ScanSync] [Source directory] [Destination directory]");
				return;
			}

			if (new[] { CommandEnum.Scan, CommandEnum.Verify }.Contains(command) && args.Length != 2)
			{
				Console.WriteLine("MX.Images [Scan | Verify] [Source directory]");
				return;
			}

			await MainAsync(
				await GetMediatorAsync(),
				command,
				queue.Dequeue(),
				isSync ? queue.Dequeue() : default);
		}

		private static Task<IMediator> GetMediatorAsync()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new Options()).As<IOptions>();
			builder.RegisterType<Storage>().As<IStorage>();
			builder.AddMediatR(typeof(Program).Assembly);

			return Task.FromResult(builder.Build().Resolve<IMediator>());
		}

		private static async Task MainAsync(IMediator mediator, CommandEnum command, string sourcePath, string destinationPath)
		{
			var tickCount = Environment.TickCount;

			switch (command)
			{
				case CommandEnum.Scan:
					await mediator.Send(new RootScanCommand(sourcePath));
					break;

				case CommandEnum.Sync:
					await mediator.Send(new RefactorCommand(sourcePath, destinationPath));
					break;

				case CommandEnum.ScanSync:
					await mediator.Send(new RootScanCommand(sourcePath));
					await mediator.Send(new RefactorCommand(sourcePath, destinationPath));
					break;

				case CommandEnum.Verify:
					await mediator.Send(new VerifyCommand(sourcePath));
					break;
			}


			Console.WriteLine($"TickCount: {Environment.TickCount - tickCount}");
		}
	}
}