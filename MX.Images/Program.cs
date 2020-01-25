using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MX.Images.Commands;
using MX.Images.Containers;
using MX.Images.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MX.Images.Commands.Refactor;
using MX.Images.Commands.Scan;

namespace MX.Images
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("MX.Images [Source directory] [Destination directory]");
				return;
			}

			var queue = new Queue<string>(args);
			await MainAsync(await GetMediatorAsync(), queue.Dequeue(), queue.Dequeue());
		}

		private static Task<IMediator> GetMediatorAsync()
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new Options()).As<IOptions>();
			builder.RegisterType<Storage>().As<IStorage>();
			builder.AddMediatR(typeof(Program).Assembly);

			return Task.FromResult(builder.Build().Resolve<IMediator>());
		}

		private static async Task MainAsync(IMediator mediator, string sourcePath, string destinationPath)
		{
			var tickCount = Environment.TickCount;

			await mediator.Send(new RootScanCommand(sourcePath));
			await mediator.Send(new RefactorCommand(sourcePath, destinationPath));

			Console.WriteLine($"TickCount: {Environment.TickCount - tickCount}");
		}
	}
}