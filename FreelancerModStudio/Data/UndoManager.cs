using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio.Data
{
    public class UndoManager<T>
    {
        public delegate void DataChangedType(List<T> o, bool undo);
        public DataChangedType DataChanged;

        private List<List<T>> Changes { get; set; }
        private int Current { get; set; }

        public UndoManager()
        {
            Changes = new List<List<T>>();
        }

        private void OnDataChanged(List<T> o, bool undo)
        {
            if (this.DataChanged != null)
                this.DataChanged(o, undo);
        }

        public List<T> CurrentData
        {
            get
            {
                if (Current > Changes.Count)
                    return null;

                return Changes[Current - 1];
            }
            set
            {
                Changes[Current - 1] = value;
            }
        }

        public void Undo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (Current > 0)
                {
                    Current--;
                    List<T> data = Changes[Current];

                    OnDataChanged(data, true);
                }
            }
        }

        public void Redo(int levels)
        {
            for (int i = 0; i < levels; i++)
            {
                if (Current < Changes.Count)
                {
                    List<T> data = Changes[Current];
                    Current++;

                    OnDataChanged(data, false);
                }
            }
        }

        public void Execute(T o)
        {
            List<T> newData = new List<T> { o };

            //remove every data which comes after the current
            Changes.RemoveRange(Current, Changes.Count - Current);

            //add the data
            Changes.Add(newData);
            Current++;

            //raise event that the data was changed
            OnDataChanged(newData, false);
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
