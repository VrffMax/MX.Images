using MediatR;
using MX.Images.Commands;
using MX.Images.Interfaces;
using MX.Images.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MX.Images.CommandHandlers
{
	public class DirectoryScanCommandHandler
		: IRequestHandler<DirectoryScanCommand, DirectoryModel>
	{
		private readonly IOptions _options;

		public DirectoryScanCommandHandler(IOptions options) =>
			_options = options;

		public async Task<DirectoryModel> Handle(DirectoryScanCommand request,
			CancellationToken cancellationToken)
		{
			try
			{
				var directoriesTask = Task.Run(() =>
					Directory.GetDirectories(request.SourcePath), cancellationToken);

				var fileTasks = _options.SearchPatterns.Select(searchPattern =>
					Task.Run(() => Directory.GetFiles(request.SourcePath, searchPattern), cancellationToken)).ToArray();

				await Task.WhenAll(
					directoriesTask,
					Task.WhenAll(fileTasks));

				return new DirectoryModel(
					Array.AsReadOnly(await directoriesTask),
					Array.AsReadOnly(fileTasks.SelectMany(fileTask => fileTask.Result).ToArray()));
			}
			catch (UnauthorizedAccessException)
			{
				return new DirectoryModel();
			}
		}
	}
}