using System.Collections.ObjectModel;

namespace MX.Images.Model.CQRS
{
    public class RefactorDirectoryModel
    {
        public string Path { get; set; }

        public ReadOnlyCollection<RefactorFileModel> Files { get; set; }
    }
}