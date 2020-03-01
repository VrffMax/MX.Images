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
            var fileMoveTasks = request.Files.Select(file =>
            {
                var fileMovePath = file.Path.Replace(request.SourcePath, request.DestinationPath);

                var sourceFile = Path.Combine(file.Path, file.Name);
                var destinationFile = Path.Combine(fileMovePath, file.Name);

                return FileMove(file.Id, fileMovePath, sourceFile, destinationFile, cancellationToken);
            }).ToArray();

            await Task.WhenAll(fileMoveTasks);

            return Unit.Value;
        }

        private Task FileMove(ObjectId id, string fileMovePath, string sourceFile, string destinationFile,
            CancellationToken cancellationToken)
        {
            try
            {
                Directory.CreateDirectory(fileMovePath);
                File.Move(sourceFile, destinationFile);

                var filter = Builders<FileModel>.Filter.Where(fileModel => fileModel.Id == id);
                var update = Builders<FileModel>.Update.Set(fileModel => fileModel.Path, fileMovePath);

                _storage.Images.Value.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                _state.Log(nameof(ExcludeCursorCommandHandler), exception.Message);
            }

            return Task.CompletedTask;
        }
    }
}