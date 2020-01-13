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
        private readonly Regex gpsDateRegex = new Regex(@"^\d{4}:\d{2}:\d{3}$", RegexOptions.Compiled | RegexOptions.Multiline);

        public Task<Unit> Handle(RefactorItemCommand request, CancellationToken cancellationToken)
        {
            var fileMakeTag = request.File.Tags.FirstOrDefault(tag => true
                                                                      && tag.Directory == "Exif IFD0"
                                                                      && tag.Name == "Make");

            var fileModelTag = request.File.Tags.FirstOrDefault(tag => true
                                                                       && tag.Directory == "Exif IFD0"
                                                                       && tag.Name == "Model");

            var fileNameTag = request.File.Tags.First(tag => true
                                                             && tag.Directory == "File"
                                                             && tag.Name == "File Name");

            var makeModelDirectory = fileMakeTag?.Description != default && fileModelTag?.Description != default
                ? $"{fileMakeTag.Description.Trim()} {fileModelTag.Description.Trim()}"
                : "NoName";

            var dateTimeTags = request.File.Tags
                .Where(tag => true
                    && tag.Name.ToLower().Contains("date")
                    && !new[] { string.Empty, "0", "0000:00:00 00:00:00" }.Contains(tag.Description))
                .Select(tag =>
                {
                    var dateTime = default(DateTime);

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "ddd MMM dd HH:mm:ss zzz yyyy",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy:MM:dd HH:mm:ss",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy:MM:dd",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy-MM-ddTHH:mm:sszzz",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy-MM-dd",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy/MM/dd HH:mm:ss",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy:MM:dd h:mm:ss tt",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParseExact(
                        tag.Description,
                        "yyyy:MM:dd HH:mm: s",
                        CultureInfo.CurrentUICulture,
                        DateTimeStyles.None,
                        out dateTime))
                    {
                        return dateTime;
                    }

                    if (gpsDateRegex.IsMatch(tag.Description))
                    {
                        var gpsDateParts = tag.Description.Split(new[] { ':' });
                        dateTime = new DateTime(int.Parse(gpsDateParts[0]), 1, 1).AddDays(int.Parse(gpsDateParts[2]) - 1);
                        return dateTime;
                    }

                    Console.WriteLine($"*** {tag.Directory} {tag.Name} ({tag.Description})");
                    return dateTime;
                })
                .ToArray();

            Console.WriteLine($"{makeModelDirectory} - {dateTimeTags.Min()} - {fileNameTag.Description}");

            return Task.FromResult(Unit.Value);
        }
    }
}