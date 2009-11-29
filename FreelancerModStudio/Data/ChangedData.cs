using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio.Data
{
    public class ChangedData
    {
        public TableBlock[] NewBlocks { get; set; }
        public TableBlock[] OldBlocks { get; set; }
        public ChangedType Type { get; set; }
    }

    public enum ChangedType { Add, Edit, Delete }
}
