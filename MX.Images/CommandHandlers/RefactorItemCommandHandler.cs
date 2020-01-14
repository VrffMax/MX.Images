using System;
using System.Collections.ObjectModel;
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
        private readonly ReadOnlyCollection<(string Pattern, CultureInfo CultureInfo)> _dateFormats = Array.AsReadOnly(
            new[]
            {
                ("ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.CurrentCulture),
                ("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture),
                ("yyyy:MM:dd", CultureInfo.InvariantCulture),
                ("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                ("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                ("yyyy:MM:dd h:mm:ss tt", CultureInfo.InvariantCulture),
                ("yyyy:MM:dd HH:mm: s", CultureInfo.InvariantCulture)
            });

        private readonly ReadOnlyCollection<(Regex Regex, Func<string, DateTime> Parse)> _dateExpressions =
            Array.AsReadOnly(new (Regex, Func<string, DateTime>)[]
            {
                (
                    new Regex(
                        @"^\d{4}:\d{2}:\d{3}$",
                        RegexOptions.Compiled | RegexOptions.Multiline),
                    value =>
                    {
                        var dateParts = value.Split(new[] {':'});

                        var dateTime = new DateTime(int.Parse(dateParts[0]), 1, 1)
                            .AddDays(int.Parse(dateParts[2]) - 1);

                        return dateTime;
                    }
                )
            });

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

            var dateTimeTags = request.File.Tags
                .Where(tag => true
                              && tag.Name.Contains("Date")
                              && !new[] { string.Empty, "0", "0000:00:00 00:00:00" }.Contains(tag.Description))
                .Select(tag =>
                {
                    var dateTime = default(DateTime);

                    if (_dateFormats.Any(dateFormat =>
                        DateTime.TryParseExact(
                            tag.Description,
                            dateFormat.Pattern,
                            dateFormat.CultureInfo,
                            DateTimeStyles.None,
                            out dateTime)))
                    {
                        return dateTime;
                    }

                    var dateFunc = _dateExpressions.FirstOrDefault(item =>
                        item.Regex.IsMatch(tag.Description));

                    if (dateFunc != default)
                    {
                        dateTime = dateFunc.Parse(tag.Description);
                        return dateTime;
                    }

                    Console.WriteLine($"*** {tag.Directory} {tag.Name} ({tag.Description})");
                    return dateTime;
                })
                .ToArray();

            var makeModelDirectory = fileMakeTag?.Description != default && fileModelTag?.Description != default
                ? $"{fileMakeTag.Description.Trim()} {fileModelTag.Description.Trim()}"
                : "NoName";

            // Console.WriteLine($"{makeModelDirectory} - {dateTimeTags.Min()} - {fileNameTag.Description}");

            return Task.FromResult(Unit.Value);
        }
    }
}