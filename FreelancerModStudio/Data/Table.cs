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
        T ID { get; set; }
    }

    [Serializable]
    public class Table<TKey, TValue> : IEnumerable<TValue>
    {
        SortedList<TKey, TValue> dictionary = new SortedList<TKey, TValue>();

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
