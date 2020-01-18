using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MX.Images.Models
{
    public class RefactorFileModel
    {
        public string Name { get; set; }

        public ReadOnlyCollection<FileModel> Sources { get; set; }
    }
}