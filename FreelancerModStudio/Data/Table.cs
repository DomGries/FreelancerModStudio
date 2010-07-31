using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace FreelancerModStudio.Data
{
    public interface ITableRow<T>
    {
        T ID { get; }
    }

    [Serializable]
    public class Table<TKey, TValue> : IEnumerable<TValue>
    {
        SortedList<TKey, TValue> dictionary;

        public Table()
        {
            dictionary = new SortedList<TKey, TValue>();
        }

        public Table(IComparer<TKey> comparer)
        {
            dictionary = new SortedList<TKey, TValue>(comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                dictionary[key] = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value, out int index)
        {
            index = dictionary.IndexOfKey(key);
            if (index != -1)
            {
                value = dictionary.Values[index];
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public TKey KeyOf(int index)
        {
            return dictionary.Keys[index];
        }

        public int IndexOf(TValue value)
        {
            return dictionary.IndexOfValue(value);
        }

        public int IndexOf(TKey key)
        {
            return dictionary.IndexOfKey(key);
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
            dictionary.Add(((ITableRow<TKey>)value).ID, value);
        }

        public void AddRange(IList<TValue> values)
        {
            foreach (TValue value in values)
                Add(value);
        }

        public void Remove(TValue value)
        {
            dictionary.Remove(((ITableRow<TKey>)value).ID);
        }

        public void RemoveAt(int index)
        {
            dictionary.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public IList<TValue> Values
        {
            get
            {
                return dictionary.Values;
            }
        }

        public System.Collections.IList List
        {
            get
            {
                return (System.Collections.IList)Values;
            }
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }
    }
}
