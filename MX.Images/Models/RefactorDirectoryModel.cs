using System.Collections.Generic;

namespace MX.Images.Models
{
    public class RefactorDirectoryModel
    {
        public string Path { get; set; }

        public IEnumerable<RefactorFileModel> Files { get; set; }
    }
}