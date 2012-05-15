using System.Collections.Generic;

namespace FreelancerModStudio.Data
{
    public class UndoManager<T>
    {
        public delegate void DataChangedType(List<T> o, bool undo);

        public DataChangedType DataChanged;

        readonly List<List<T>> _changes = new List<List<T>>();
        int _current;
        int _savedIndex;

        void OnDataChanged(List<T> o, bool undo)
        {
            if (DataChanged != null)
            {
                DataChanged(o, undo);
            }
        }

        public List<T> CurrentData
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

        public void Undo(int levels)
        {
            for (int i = 0; i < levels; ++i)
            {
                if (_current > 0)
                {
                    _current--;
                    List<T> data = _changes[_current];

                    OnDataChanged(data, true);
                }
            }
        }

        public void Redo(int levels)
        {
            for (int i = 0; i < levels; ++i)
            {
                if (_current < _changes.Count)
                {
                    List<T> data = _changes[_current];
                    ++_current;

                    OnDataChanged(data, false);
                }
            }
        }

        public void Execute(T o)
        {
            List<T> newData = new List<T>
                {
                    o
                };

            //remove every data which comes after the current
            _changes.RemoveRange(_current, _changes.Count - _current);

            //add the data
            _changes.Add(newData);
            ++_current;

            //raise event that the data was changed
            OnDataChanged(newData, false);
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
