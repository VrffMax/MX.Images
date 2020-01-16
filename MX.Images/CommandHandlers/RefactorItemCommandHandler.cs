using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands;
using MX.Images.Models;

namespace MX.Images.CommandHandlers
{
    public class RefactorItemCommandHandler
        : IRequestHandler<RefactorItemCommand>
    {
        private readonly ReadOnlyCollection<string> _excludeDateTimes =
            Array.AsReadOnly(new[]
            {
                string.Empty,
                "0",
                "0000:00:00 00:00:00"
            });

        private readonly ReadOnlyCollection<(string Pattern, CultureInfo CultureInfo)> _dateTimeFormats =
            Array.AsReadOnly(new[]
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

        private readonly ReadOnlyCollection<(Regex Regex, Func<string, DateTime> Parse)> _dateTimeExpressions =
            Array.AsReadOnly(new (Regex, Func<string, DateTime>)[]
            {
                (
                    // GPS
                    new Regex(@"^\d{4}:\d{2}:\d{3}$", RegexOptions.Compiled),
                    value =>
                    {
                        var dateParts = value.Split(new[] {':'});

                        var date = new DateTime(int.Parse(dateParts[0]), 1, 1)
                            .AddDays(int.Parse(dateParts[2]) - 1);

                        return date;
                    }
                )
            });

        private FileModelTag GetFileMakeTag(RefactorItemCommand request) =>
            request.File.Tags.FirstOrDefault(tag => tag.Directory == "Exif IFD0" && tag.Name == "Make");

        private FileModelTag GetFileModelTag(RefactorItemCommand request) =>
            request.File.Tags.FirstOrDefault(tag => tag.Directory == "Exif IFD0" && tag.Name == "Model");

        private FileModelTag GetFileNameTag(RefactorItemCommand request) =>
            request.File.Tags.FirstOrDefault(tag => tag.Directory == "File" && tag.Name == "File Name");

        private DateTime GetDateTimeMin(RefactorItemCommand request) =>
            request.File.Tags
                .Where(tag => tag.Name.Contains("Date") && !_excludeDateTimes.Contains(tag.Description))
                .Select(tag =>
                {
                    var dateTime = default(DateTime);

                    if (_dateTimeFormats.Any(dateFormat =>
                        DateTime.TryParseExact(
                            tag.Description,
                            dateFormat.Pattern,
                            dateFormat.CultureInfo,
                            DateTimeStyles.None,
                            out dateTime)))
                    {
                        return dateTime;
                    }

                    var dateTimeFunc = _dateTimeExpressions.FirstOrDefault(item => item.Regex.IsMatch(tag.Description));

                    if (dateTimeFunc != default)
                    {
                        dateTime = dateTimeFunc.Parse(tag.Description);
                        return dateTime;
                    }

                    Console.WriteLine($"*** {tag.Directory} {tag.Name} ({tag.Description})");
                    return dateTime;
                })
                .Where(dateTime => dateTime != default)
                .Min();

        public Task<Unit> Handle(RefactorItemCommand request, CancellationToken cancellationToken)
        {
            var fileMakeTag = GetFileMakeTag(request);
            var fileModelTag = GetFileModelTag(request);

            var makeModelDirectory = fileMakeTag?.Description != default && fileModelTag?.Description != default
                ? $"{fileMakeTag.Description} {fileModelTag.Description}"
                : "NoName";

            var fileNameTag = GetFileNameTag(request);
            var dateTimeMin = GetDateTimeMin(request);

            Console.WriteLine($"{makeModelDirectory} - {dateTimeMin} - {fileNameTag.Description}");

            if (dateTimeMin == default)
            {
                Console.WriteLine($"???");
            }

            return Task.FromResult(Unit.Value);
        }
    }
}