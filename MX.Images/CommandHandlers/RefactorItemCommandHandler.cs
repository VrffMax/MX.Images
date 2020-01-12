using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands;

namespace MX.Images.CommandHandlers
{
    public class RefactorItemCommandHandler
        : IRequestHandler<RefactorItemCommand>
    {
        private readonly Regex dateTimeRegex;

        public RefactorItemCommandHandler()
        {
            dateTimeRegex = new Regex(@" \d{2}:\d{2}", RegexOptions.Compiled);
        }

        public Task<Unit> Handle(RefactorItemCommand request, CancellationToken cancellationToken)
        {
            var fileMakeTag = request.File.Tags.FirstOrDefault(tag => true
                                                                      && tag.Directory == "Exif IFD0"
                                                                      && tag.Name == "Make");

            var fileModelTag = request.File.Tags.FirstOrDefault(tag => true
                                                                       && tag.Directory == "Exif IFD0"
                                                                       && tag.Name == "Model");

            var dateTimeTags = request.File.Tags
                .Where(tag => tag.Name.ToLower().Contains("date"))
                .Select(tag =>
                {
                    if (DateTime.TryParseExact(
                        tag.Description,
                        "ddd MMM dd HH:mm:ss zzz yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy:MM:dd HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    Console.WriteLine($"*** {tag.Description}");
                    return default;
                })
                .ToArray();

            var fileNameTag = request.File.Tags.First(tag => true
                                                             && tag.Directory == "File"
                                                             && tag.Name == "File Name");

            var makeModelDirectory = fileMakeTag?.Description != default && fileModelTag?.Description != default
                ? $"{fileMakeTag.Description.Trim()} {fileModelTag.Description.Trim()}"
                : "NoName";

            Console.WriteLine($"{makeModelDirectory} - {dateTimeTags.Min()} - {fileNameTag.Description}");

            return Task.FromResult(Unit.Value);
        }
    }
}