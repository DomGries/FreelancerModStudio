namespace FreelancerModStudio.Data
{
    using System.Collections.Generic;

    public class UndoManager<T> where T : class
    {
        public delegate void DataChangedType(T o, bool undo);

        public DataChangedType DataChanged;

        private readonly List<T> changes = new List<T>();

        private int current;

        private int savedIndex;

        private void OnDataChanged(T o, bool undo)
        {
            this.DataChanged?.Invoke(o, undo);
        }

        public T CurrentData
        {
            get
            {
                if (this.current > this.changes.Count)
                {
                    return null;
                }

                return this.changes[this.current - 1];
            }

            set
            {
                this.changes[this.current] = value;
            }
        }

        public void Undo(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                if (this.current > 0)
                {
                    this.current--;
                    T data = this.changes[this.current];

                    this.OnDataChanged(data, true);
                }
            }
        }

        public void Redo(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                if (this.current < this.changes.Count)
                {
                    T data = this.changes[this.current];
                    ++this.current;

                    this.OnDataChanged(data, false);
                }
            }
        }

        public void Execute(T o)
        {
            // remove every data which comes after the current
            this.changes.RemoveRange(this.current, this.changes.Count - this.current);

            // add the data
            this.changes.Add(o);
            ++this.current;

            // raise event that the data was changed
            this.OnDataChanged(o, false);
        }

        public bool CanUndo()
        {
            return this.current > 0;
        }

        public bool CanRedo()
        {
            return this.changes.Count - this.current > 0;
        }

        public void SetAsSaved()
        {
            this.savedIndex = this.current;
        }

        public bool IsModified()
        {
            return this.savedIndex != this.current;
        }
    }
}
