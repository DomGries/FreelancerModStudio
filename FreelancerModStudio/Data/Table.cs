namespace FreelancerModStudio.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public class Table<TKey, TValue> : IEnumerable<TValue> where TValue : class
    {
        private readonly SortedList<TKey, TValue> dictionary;

        public Table()
        {
            this.dictionary = new SortedList<TKey, TValue>();
        }

        public Table(IComparer<TKey> comparer)
        {
            this.dictionary = new SortedList<TKey, TValue>(comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }

            set
            {
                this.dictionary[key] = value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value, out int index)
        {
            index = this.dictionary.IndexOfKey(key);
            if (index != -1)
            {
                value = this.dictionary.Values[index];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public TKey KeyOf(int index)
        {
            return this.dictionary.Keys[index];
        }

        public int IndexOf(TValue value)
        {
            return this.dictionary.IndexOfValue(value);
        }

        public int IndexOf(TKey key)
        {
            return this.dictionary.IndexOfKey(key);
        }

        public bool Contains(TValue value)
        {
            return this.IndexOf(value) != -1;
        }

        public bool Contains(TKey key)
        {
            return this.IndexOf(key) != -1;
        }

        public void Add(TValue value)
        {
            this.dictionary.Add(((ITableRow<TKey>)value).Id, value);
        }

        public void AddRange(IList<TValue> values)
        {
            foreach (TValue value in values)
            {
                this.Add(value);
            }
        }

        public void Remove(TValue value)
        {
            this.dictionary.Remove(((ITableRow<TKey>)value).Id);
        }

        public void RemoveAt(int index)
        {
            this.dictionary.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public IList<TValue> Values
        {
            get
            {
                return this.dictionary.Values;
            }
        }

        public IList List
        {
            get
            {
                return (IList)this.Values;
            }
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return this.dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionary.Values.GetEnumerator();
        }
    }
}
