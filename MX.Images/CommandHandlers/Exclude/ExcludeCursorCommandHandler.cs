using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MX.Images.Commands.Exclude;
using MX.Images.Interfaces;
using MX.Images.Models;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Exclude
{
    public class ExcludeCursorCommandHandler
        : IRequestHandler<ExcludeCursorCommand, Unit>
    {
        private readonly IState _state;
        private readonly IStorage _storage;

        public ExcludeCursorCommandHandler(IState state, IStorage storage)
        {
            _state = state;
            _storage = storage;
        }

        public async Task<Unit> Handle(ExcludeCursorCommand request, CancellationToken cancellationToken)
        {
            var fileGroups = request.Files
                .GroupBy(file => file.Path)
                .Select(files => new
                {
                    Files = files,
                    MovePath = files.Key.Replace(request.SourcePath, request.DestinationPath)
                })
                .ToArray();

            var directoryCreateTasks = fileGroups.Select(fileGroup =>
            {
                Directory.CreateDirectory(fileGroup.MovePath);
                return Task.CompletedTask;
            }).ToArray();

            await Task.WhenAll(directoryCreateTasks);

            var fileMoveTasks = fileGroups.SelectMany(fileGroup =>
                    fileGroup.Files.Select(file =>
                            FileMove(file.Id, file.Path, fileGroup.MovePath, file.Name))
                        .ToArray())
                .ToArray();

            await Task.WhenAll(fileMoveTasks);

            return Unit.Value;
        }

        private async Task FileMove(ObjectId id, string path, string movePath, string name)
        {
            try
            {
                var sourceFile = Path.Combine(path, name);
                var destinationFile = Path.Combine(movePath, name);

                File.Move(sourceFile, destinationFile);
                await FileUpdate(id, movePath);
            }
            catch (Exception exception)
            {
                _state.Log(nameof(ExcludeCursorCommandHandler), exception);
            }
        }

        private Task FileUpdate(ObjectId id, string movePath)
        {
            var filter = Builders<FileModel>.Filter.Where(fileModel => fileModel.Id == id);
            var update = Builders<FileModel>.Update.Set(fileModel => fileModel.Path, movePath);

            return _storage.Images.Value.UpdateOneAsync(filter, update);
        }
    }
}