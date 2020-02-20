using System.Collections.ObjectModel;
using MX.Images.Models.Mongo;

namespace MX.Images.Models.CQRS
{
    public class RefactorFileModel
    {
        public string Name { get; set; }

        public ReadOnlyCollection<FileModel> Sources { get; set; }
    }
}