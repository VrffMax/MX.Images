using System;
using MX.Images.Model.Mongo;

namespace MX.Images.Model.CQRS
{
    public class RefactorItemModel
    {
        public string MakeModelDirectory { get; set; }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        public FileModel File { get; set; }
    }
}