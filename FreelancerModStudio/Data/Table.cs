using System;
using System.Collections;
using System.Collections.Generic;

namespace FreelancerModStudio.Data
{
    [Serializable]
    public class Table<TKey, TValue> : IEnumerable<TValue> where TValue : class
    {
        readonly SortedList<TKey, TValue> _dictionary;

        public Table()
        {
            _dictionary = new SortedList<TKey, TValue>();
        }

        public Table(IComparer<TKey> comparer)
        {
            _dictionary = new SortedList<TKey, TValue>(comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value, out int index)
        {
            index = _dictionary.IndexOfKey(key);
            if (index != -1)
            {
                value = _dictionary.Values[index];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public TKey KeyOf(int index)
        {
            return _dictionary.Keys[index];
        }

        public int IndexOf(TValue value)
        {
            return _dictionary.IndexOfValue(value);
        }

        public int IndexOf(TKey key)
        {
            return _dictionary.IndexOfKey(key);
        }

        public bool Contains(TValue value)
        {
            return IndexOf(value) != -1;
        }

        public bool Contains(TKey key)
        {
            return IndexOf(key) != -1;
        }

        public void Add(TValue value)
        {
            _dictionary.Add(((ITableRow<TKey>)value).Id, value);
        }

        public void AddRange(IList<TValue> values)
        {
            foreach (TValue value in values)
            {
                Add(value);
            }
        }

        public void Remove(TValue value)
        {
            _dictionary.Remove(((ITableRow<TKey>)value).Id);
        }

        public void RemoveAt(int index)
        {
            _dictionary.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public IList<TValue> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        public IList List
        {
            get
            {
                return (IList)Values;
            }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }
    }
}
