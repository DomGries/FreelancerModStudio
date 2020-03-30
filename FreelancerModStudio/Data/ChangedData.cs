namespace FreelancerModStudio.Data
{
    using System.Collections.Generic;

    public class ChangedData
    {
        public List<TableBlock> NewBlocks { get; set; }
        public List<TableBlock> NewAdditionalBlocks { get; set; }
        public List<TableBlock> OldBlocks { get; set; }
        public ChangedType Type { get; set; }

        public ChangedData GetUndoData()
        {
            switch (this.Type)
            {
                case ChangedType.Add:
                    return new ChangedData
                        {
                            Type = ChangedType.Delete,
                            NewBlocks = this.NewBlocks,
                        };
                case ChangedType.Delete:
                    return new ChangedData
                        {
                            Type = ChangedType.Add,
                            NewBlocks = this.NewBlocks,
                        };
                case ChangedType.AddAndEdit:
                    return new ChangedData
                        {
                            Type = ChangedType.DeleteAndEdit,
                            NewBlocks = this.NewBlocks,
                            OldBlocks = this.NewAdditionalBlocks,
                            NewAdditionalBlocks = this.OldBlocks,
                        };
                case ChangedType.DeleteAndEdit:
                    return new ChangedData
                        {
                            Type = ChangedType.AddAndEdit,
                            NewBlocks = this.NewBlocks,
                            OldBlocks = this.NewAdditionalBlocks,
                            NewAdditionalBlocks = this.OldBlocks,
                        };
                default:
                    return new ChangedData
                        {
                            Type = this.Type,
                            NewBlocks = this.OldBlocks,
                            OldBlocks = this.NewBlocks,
                        };
            }
        }
    }
}
