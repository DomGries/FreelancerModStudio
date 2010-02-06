using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio.Data
{
    public class ChangedData
    {
        public List<TableBlock> NewBlocks { get; set; }
        public List<TableBlock> OldBlocks { get; set; }
        public ChangedType Type { get; set; }

        public ChangedData GetUndoData()
        {
            if (Type == ChangedType.Add)
            {
                return new ChangedData()
                {
                    Type = ChangedType.Delete,
                    NewBlocks = this.NewBlocks
                };
            }
            else if (Type == ChangedType.Delete)
            {
                return new ChangedData()
                {
                    Type = ChangedType.Add,
                    NewBlocks = this.NewBlocks
                };
            }
            else
            {
                return new ChangedData()
                {
                    Type = this.Type,
                    NewBlocks = this.OldBlocks,
                    OldBlocks = this.NewBlocks
                };
            }
        }
    }

    public enum ChangedType { Add, Edit, Delete }
}
