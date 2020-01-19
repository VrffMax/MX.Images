using MediatR;
using MongoDB.Driver;
using MX.Images.Commands;
using MX.Images.Interfaces;
using MX.Images.Models;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MX.Images.CommandHandlers
{
	public class RefactorCopyCommandHandler
		: IRequestHandler<RefactorCopyCommand>
	{
		private readonly IStorage _storage;

		public RefactorCopyCommandHandler(IStorage storage) =>
			_storage = storage;

		public async Task<Unit> Handle(RefactorCopyCommand request, CancellationToken cancellationToken)
		{
			Directory.CreateDirectory(request.RefactorDirectory.Path);

			var filesTasks = request.RefactorDirectory.Files.Select(file =>
			{
				var destinationPath = Path.Combine(request.RefactorDirectory.Path, file.Name);

				if (file.Sources.Count() == 1)
				{
					var source = file.Sources.Single();
					var sourceFile = Path.Combine(source.Path, source.Name);

					if (File.Exists(sourceFile))
					{
						if (File.Exists(destinationPath) && source.LastWriteTimeUtc == File.GetLastWriteTimeUtc(sourceFile))
						{
							return Task.CompletedTask;
						}

						return Task.Run(() => File.Copy(sourceFile, destinationPath, true), cancellationToken);
					}

					var deleteTask = Directory.Exists(destinationPath)
						? Task.Run(() => Directory.Delete(destinationPath, true), cancellationToken)
						: File.Exists(destinationPath)
							? Task.Run(() => File.Delete(destinationPath), cancellationToken)
							: Task.CompletedTask;

					var filter = Builders<FileModel>.Filter.Where(fileModel => fileModel.Id == source.Id);

					return Task.WhenAll(
						deleteTask,
						_storage.Images.Value.DeleteOneAsync(filter, cancellationToken));
				}

				Directory.CreateDirectory(destinationPath);

				var counter = 1;
				var extension = Path.GetExtension(file.Name);

				var sourcesTasks = file.Sources.Select(source =>
				{
					var sourceFile = Path.Combine(source.Path, source.Name);
					var destinationFile = Path.Combine(destinationPath, $"{counter++}{extension}");

					return Task.Run(() =>
						File.Copy(sourceFile, destinationFile), cancellationToken);
				}).ToArray();

				return Task.WhenAll(sourcesTasks);
			}).ToArray();

			await Task.WhenAll(filesTasks);

			return Unit.Value;
		}
	}
}