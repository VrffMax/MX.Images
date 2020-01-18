using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands;

namespace MX.Images.CommandHandlers
{
    public class RefactorCopyCommandHandler
        : IRequestHandler<RefactorCopyCommand>
    {
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

                    return Task.Run(() =>
                        File.Copy(sourceFile, destinationPath), cancellationToken);
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