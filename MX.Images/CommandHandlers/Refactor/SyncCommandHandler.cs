using MediatR;
using MongoDB.Driver;
using MX.Images.Commands.Refactor;
using MX.Images.Interfaces;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MX.Images.CommandHandlers.Refactor
{
	public class SyncCommandHandler
		: IRequestHandler<SyncCommand>
	{
		private readonly IMediator _mediator;
		private readonly IStorage _storage;
		private readonly IOptions _options;

		public SyncCommandHandler(
			IMediator mediator,
			IStorage storage,
			IOptions options)
		{
			_mediator = mediator;
			_storage = storage;
			_options = options;
		}

		public async Task<Unit> Handle(SyncCommand request, CancellationToken cancellationToken)
		{
			var findFilter = Builders<FileModel>.Filter.Where(fileModel => true
																		   && fileModel.Machine == _options.Machine
																		   && fileModel.Path.StartsWith(
																			   request.SourcePath));

			var findOptions = new FindOptions<FileModel, FileModel>
			{
				Projection = Builders<FileModel>.Projection
					.Include(fileModel => fileModel.Id)
					.Include(fileModel => fileModel.Path)
					.Include(fileModel => fileModel.Name)
					.Include(fileModel => fileModel.LastWriteTimeUtc)
					.Include(fileModel => fileModel.Tags)
			};

			var refactorCursorTasks = new Task<ReadOnlyCollection<RefactorItemModel>>[0];
			var filesCursor = await _storage.Images.Value.FindAsync(findFilter, findOptions, cancellationToken);

			while (await filesCursor.MoveNextAsync(cancellationToken))
			{
				refactorCursorTasks = refactorCursorTasks.Append(_mediator.Send(
					new SyncCursorCommand(Array.AsReadOnly(filesCursor.Current.ToArray())),
					cancellationToken)).ToArray();
			}

			await Task.WhenAll(refactorCursorTasks);

			var refactorDirectories = await _mediator.Send(
				new SyncMapCommand(
					request.DestinationPath,
					Array.AsReadOnly(refactorCursorTasks.SelectMany(refactorItemTask => refactorItemTask.Result)
						.ToArray())),
				cancellationToken);

			var refactorCopyTasks = refactorDirectories.Select(refactorDirectory =>
				_mediator.Send(new SyncCopyCommand(refactorDirectory), cancellationToken)).ToArray();

			await Task.WhenAll(refactorCopyTasks);

			return Unit.Value;
		}
	}
}