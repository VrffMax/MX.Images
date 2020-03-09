using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MX.Images.Commands.Sync;
using MX.Images.Models;
using MX.Images.Models.CQRS;
using MX.Images.Models.Mongo;

namespace MX.Images.CommandHandlers.Sync
{
    public class SyncCursorCommandHandler
        : IRequestHandler<SyncCursorCommand, ReadOnlyCollection<RefactorItemModel>>
    {
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

        private readonly ReadOnlyCollection<string> _excludeDateTimes =
            Array.AsReadOnly(new[]
            {
                string.Empty,
                "0",
                "0000:00:00 00:00:00"
            });

        private readonly IState _state;

        public SyncCursorCommandHandler(IState state)
        {
            _state = state;
        }

        public async Task<ReadOnlyCollection<RefactorItemModel>> Handle(SyncCursorCommand request,
            CancellationToken cancellationToken)
        {
            var getRefactorItemModelTasks =
                request.Files.Select(file => GetRefactorItemModel(file, cancellationToken)).ToArray();

            await Task.WhenAll(getRefactorItemModelTasks);

            return Array.AsReadOnly(getRefactorItemModelTasks
                .Select(getRefactorItemModelTask => getRefactorItemModelTask.Result).ToArray());
        }

        private Task<RefactorItemModel> GetRefactorItemModel(FileModel file, CancellationToken cancellationToken)
        {
            var fileMakeTag = GetFileMakeTag(file.Tags);
            var fileModelTag = GetFileModelTag(file.Tags);

            var makeModelDirectory = fileMakeTag?.Description != default && fileModelTag?.Description != default
                ? $"{fileMakeTag.Description} {fileModelTag.Description}"
                : "NoName";

            var fileNameTag = GetFileNameTag(file.Tags);

            var name = fileNameTag == default || string.IsNullOrWhiteSpace(fileNameTag.Description)
                ? $"{Guid.NewGuid().ToString().ToUpper()}{Path.GetExtension(file.Name).ToLower()}"
                : fileNameTag.Description;

            var dateTime = GetDateTime(file.Tags);

            return Task.FromResult(new RefactorItemModel
            {
                MakeModelDirectory = makeModelDirectory,
                Name = name,
                DateTime = dateTime,
                File = file
            });
        }

        private FileModelTag GetFileMakeTag(ReadOnlyCollection<FileModelTag> tags)
        {
            return tags.FirstOrDefault(tag => tag.Directory == "Exif IFD0" && tag.Name == "Make");
        }

        private FileModelTag GetFileModelTag(ReadOnlyCollection<FileModelTag> tags)
        {
            return tags.FirstOrDefault(tag => tag.Directory == "Exif IFD0" && tag.Name == "Model");
        }

        private FileModelTag GetFileNameTag(ReadOnlyCollection<FileModelTag> tags)
        {
            return tags.FirstOrDefault(tag => tag.Directory == "File" && tag.Name == "File Name");
        }

        private DateTime GetDateTime(ReadOnlyCollection<FileModelTag> tags)
        {
            return tags.Where(tag => tag.Name.Contains("Date") && !_excludeDateTimes.Contains(tag.Description))
                .Select(tag =>
                {
                    // TryParseExact stage
                    {
                        var dateTime = default(DateTime);

                        if (_dateTimeFormats.Any(dateFormat =>
                            DateTime.TryParseExact(
                                tag.Description,
                                dateFormat.Pattern,
                                dateFormat.CultureInfo,
                                DateTimeStyles.None,
                                out dateTime)))
                            return dateTime;
                    }

                    // Regex stage
                    var dateTimeParseFunc =
                        _dateTimeExpressions.FirstOrDefault(item => item.Regex.IsMatch(tag.Description));

                    if (dateTimeParseFunc != default)
                    {
                        var dateTime = dateTimeParseFunc.Parse(tag.Description);
                        return dateTime;
                    }

                    // Default stage
                    _state.Log(nameof(SyncCursorCommandHandler),
                        new ApplicationException($@"""{tag.Directory}"" & ""{tag.Name}"" & ""{tag.Description}"""));

                    return default;
                })
                .Where(dateTime => dateTime != default)
                .Min();
        }
    }
}