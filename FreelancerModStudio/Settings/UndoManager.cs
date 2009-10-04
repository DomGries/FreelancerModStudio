using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio.Settings
{
    public class UndoManager
    {
        public delegate void DataChangedType(TableBlock[] newBlocks, TableBlock[] oldBlocks, ChangedType type);
        public DataChangedType DataChanged;

        List<ChangedData> changes = new List<ChangedData>();
        int current = 0;

        private void OnDataChanged(TableBlock[] newBlocks, TableBlock[] oldBlocks, ChangedType type)
        {
            if (this.DataChanged != null)
                this.DataChanged(newBlocks, oldBlocks, type);
        }

        public void Undo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (current > 0)
                {
                    ChangedData data = changes[--current];
                    if (data.Type == ChangedType.Add)
                        OnDataChanged(data.NewBlocks, null, ChangedType.Delete);
                    else if (data.Type == ChangedType.Delete)
                        OnDataChanged(data.NewBlocks, null, ChangedType.Add);
                    else if (data.Type == ChangedType.Edit)
                        OnDataChanged(data.OldBlocks, data.NewBlocks, data.Type);
                }
            }
        }

        public void Redo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (current < changes.Count)
                {
                    ChangedData data = changes[current++];
                    OnDataChanged(data.NewBlocks, data.OldBlocks, data.Type);
                }
            }
        }

        public void Execute(TableBlock[] newBlocks, TableBlock[] oldBlocks, ChangedType type)
        {
            changes.RemoveRange(current, changes.Count - current);
            changes.Add(new ChangedData() { NewBlocks = newBlocks, OldBlocks = oldBlocks, Type = type });
            current++;

            OnDataChanged(newBlocks, oldBlocks, type);
        }

        public bool CanUndo()
        {
            return current > 0;
        }

        public bool CanRedo()
        {
            return changes.Count - current > 0;
        }

        public class ChangedData
        {
            public TableBlock[] NewBlocks { get; set; }
            public TableBlock[] OldBlocks { get; set; }
            public ChangedType Type { get; set; }
        }

        public enum ChangedType { Add, Edit, Delete }
    }
}
