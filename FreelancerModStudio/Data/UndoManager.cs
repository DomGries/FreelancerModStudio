using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio.Data
{
    public class UndoManager<T>
    {
        public delegate void DataChangedType(T o, bool undo);
        public DataChangedType DataChanged;

        public List<T[]> Changes { get; set; }
        public int Current { get; set; }

        public UndoManager()
        {
            Changes = new List<T[]>();
        }

        private void OnDataChanged(T o, bool undo)
        {
            if (this.DataChanged != null)
                this.DataChanged(o, undo);
        }

        public void Undo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (Current > 0)
                {
                    T[] data = Changes[--Current];
                    foreach (T entry in data)
                        OnDataChanged(entry, true);
                }
            }
        }

        public void Redo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (Current < Changes.Count)
                {
                    T[] data = Changes[Current++];
                    foreach (T entry in data)
                        OnDataChanged(entry, false);
                }
            }
        }

        public void Execute(T o)
        {
            Changes.RemoveRange(Current, Changes.Count - Current);
            Changes.Add(new T[] { o });
            Current++;

            OnDataChanged(o, false);
        }

        public bool CanUndo()
        {
            return Current > 0;
        }

        public bool CanRedo()
        {
            return Changes.Count - Current > 0;
        }
    }
}
