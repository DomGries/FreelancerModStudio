using System.Collections.Generic;

namespace FreelancerModStudio.Data
{
    public class UndoManager<T> where T : class
    {
        public delegate void DataChangedType(T o, bool undo);

        public DataChangedType DataChanged;

        readonly List<T> _changes = new List<T>();
        int _current;
        int _savedIndex;

        void OnDataChanged(T o, bool undo)
        {
            if (DataChanged != null)
            {
                DataChanged(o, undo);
            }
        }

        public T CurrentData
        {
            get
            {
                if (_current > _changes.Count)
                {
                    return null;
                }

                return _changes[_current - 1];
            }
            set
            {
                _changes[_current] = value;
            }
        }

        public void Undo(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                if (_current > 0)
                {
                    _current--;
                    T data = _changes[_current];

                    OnDataChanged(data, true);
                }
            }
        }

        public void Redo(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                if (_current < _changes.Count)
                {
                    T data = _changes[_current];
                    ++_current;

                    OnDataChanged(data, false);
                }
            }
        }

        public void Execute(T o)
        {
            //remove every data which comes after the current
            _changes.RemoveRange(_current, _changes.Count - _current);

            //add the data
            _changes.Add(o);
            ++_current;

            //raise event that the data was changed
            OnDataChanged(o, false);
        }

        public bool CanUndo()
        {
            return _current > 0;
        }

        public bool CanRedo()
        {
            return _changes.Count - _current > 0;
        }

        public void SetAsSaved()
        {
            _savedIndex = _current;
        }

        public bool IsModified()
        {
            return _savedIndex != _current;
        }
    }
}
