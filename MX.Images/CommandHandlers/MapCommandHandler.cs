using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class MapCommandHandler
        : IRequestHandler<MapCommand, ReadOnlyCollection<RefactorDirectoryModel>>
    {
        public Task<ReadOnlyCollection<RefactorDirectoryModel>> Handle(
            MapCommand request,
            CancellationToken cancellationToken) =>
            Task.FromResult(Array.AsReadOnly(request.RefactorItems
                .GroupBy(refactorItem => new
                {
                    refactorItem.MakeModelDirectory,
                    refactorItem.DateTime.Year,
                    Quarter = (refactorItem.DateTime.Month + 2) / 3,
                })
                .Select(refactorItemGroup =>
                    new RefactorDirectoryModel
                    {
                        Path = Path.Combine(
                            request.Path,
                            refactorItemGroup.Key.MakeModelDirectory,
                            $"{refactorItemGroup.Key.Year} {refactorItemGroup.Key.Quarter}"),
                        Files = Array.AsReadOnly(refactorItemGroup
                            .GroupBy(files => files.Name)
                            .Select(filesGroup =>
                                new RefactorFileModel
                                {
                                    Name = filesGroup.Key,
                                    Sources = Array.AsReadOnly(filesGroup
                                        .Select(file => file.File)
                                        .ToArray())
                                })
                            .ToArray())
                    })
                .ToArray()));
    }
}