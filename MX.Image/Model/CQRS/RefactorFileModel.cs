using System.Collections.ObjectModel;
using MX.Images.Model.Mongo;

namespace MX.Images.Model.CQRS
{
    public class RefactorFileModel
    {
        public string Name { get; set; }

        public ReadOnlyCollection<FileModel> Sources { get; set; }
    }
}