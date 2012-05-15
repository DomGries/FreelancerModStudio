using System.Collections.Generic;

namespace FreelancerModStudio.Data
{
    public class ChangedData
    {
        public List<TableBlock> NewBlocks { get; set; }
        public List<TableBlock> OldBlocks { get; set; }
        public ChangedType Type { get; set; }

        public ChangedData GetUndoData()
        {
            switch (Type)
            {
                case ChangedType.Add:
                    return new ChangedData
                        {
                            Type = ChangedType.Delete,
                            NewBlocks = NewBlocks
                        };
                case ChangedType.Delete:
                    return new ChangedData
                        {
                            Type = ChangedType.Add,
                            NewBlocks = NewBlocks
                        };
                default:
                    return new ChangedData
                        {
                            Type = Type,
                            NewBlocks = OldBlocks,
                            OldBlocks = NewBlocks
                        };
            }
        }
    }

    public enum ChangedType
    {
        Add,
        Edit,
        Move,
        Delete
    }
}
