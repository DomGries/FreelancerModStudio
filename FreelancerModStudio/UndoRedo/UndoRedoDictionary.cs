// This source is under LGPL license. Sergei Arhipenko (c) 2006-2007. email: sbs-arhipenko@yandex.ru. This notice may not be removed.
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace FreelancerModStudio
{
    public class UndoRedoDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IUndoRedoMember
    {
        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        /// class that is empty, has the default initial capacity, and uses the default
        /// equality comparer for the key type.
        /// </summary>
        public UndoRedoDictionary() 
        {
        }
        
        /// <summary>
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that contains elements copied from the specified System.Collections.Generic.IDictionary<TKey,TValue>
        //     and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">
        /// The System.Collections.Generic.IDictionary<TKey,TValue> whose elements are
        /// copied to the new System.Collections.Generic.Dictionary<TKey,TValue>.
        /// </param>
        public UndoRedoDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {}

        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        ///     class that is empty, has the default initial capacity, and uses the specified
        ///     System.Collections.Generic.IEqualityComparer<T>.
        /// </summary>
        /// <param name="comparer">
        /// The System.Collections.Generic.IEqualityComparer<T> implementation to use
        /// when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T>
        /// for the type of the key.
        /// </param>
        public UndoRedoDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        { }
        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        /// class that is empty, has the specified initial capacity, and uses the default
        /// equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the System.Collections.Generic.Dictionary<TKey,TValue> can contain.</param>
        public UndoRedoDictionary(int capacity) : base(capacity)
        {}
        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        /// class that contains elements copied from the specified System.Collections.Generic.IDictionary<TKey,TValue>
        /// and uses the specified System.Collections.Generic.IEqualityComparer<T>.
        /// </summary>
        /// <param name="dictionary">The System.Collections.Generic.IDictionary<TKey,TValue> whose elements are copied to the new System.Collections.Generic.Dictionary<TKey,TValue>.</param>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer<T> implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T> for the type of the key.</param>
        public UndoRedoDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {}
        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        /// class that is empty, has the specified initial capacity, and uses the specified
        /// System.Collections.Generic.IEqualityComparer<T>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the System.Collections.Generic.Dictionary<TKey,TValue> can contain.</param>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer<T> implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T> for the type of the key.</param>
        public UndoRedoDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {}
        /// <summary>
        /// Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue> class with serialized data.
        /// </summary>
        /// <param name="info">A System.Runtime.Serialization.SerializationInfo object containing the information required to serialize the System.Collections.Generic.Dictionary<TKey,TValue>.</param>
        /// <param name="context">A System.Runtime.Serialization.StreamingContext structure containing the source and destination of the serialized stream associated with the System.Collections.Generic.Dictionary<TKey,TValue>.</param>
        protected UndoRedoDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a System.Collections.Generic.KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        public new TValue this[TKey key] 
        {
            get { return base[key];  }
            set 
            {
                if (key != null)
                {
                    ChangesList changes = Enlist();
					if (changes != null)
					{
						if (!ContainsKey(key))
						{
							changes.Add(
								delegate { base[key] = value; },
								delegate { base.Remove(key); });
						}
						else
						{
							TValue oldValue = base[key];
							changes.Add(
								delegate { base[key] = value; },
								delegate { base[key] = oldValue; });
						}
					}
                }
                base[key] = value;
            }
        }

        /// <summary>Adds the specified key and value to the dictionary.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public new void Add(TKey key, TValue value)
        {
            if (key != null && !ContainsKey(key))
            {
                ChangesList changes = Enlist();
				if (changes != null)
					changes.Add(
						delegate { base.Add(key, value); },
						delegate { base.Remove(key); });
            }
            base.Add(key, value);
        }
        /// <summary>
        /// Removes all keys and values from the System.Collections.Generic.Dictionary<TKey,TValue>.
        /// </summary>
        public new void Clear() 
        {
            ChangesList changes = Enlist();
			if (changes != null)
			{
				Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>(this);
				changes.Add(
						delegate { base.Clear(); },
						delegate { foreach (TKey key in copy.Keys) { base.Add(key, copy[key]); } });
			}
            base.Clear();
        }

        /// <summary>
        /// Removes the value with the specified key from the System.Collections.Generic.Dictionary<TKey,TValue>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the System.Collections.Generic.Dictionary<TKey,TValue>.</returns>
        public new bool Remove(TKey key)
        {
            TValue value;
            if (base.TryGetValue(key, out value))
            {
                ChangesList changes = Enlist();
				if (changes != null)
					changes.Add(
						delegate { base.Remove(key); },
						delegate { base.Add(key, value); });

                return base.Remove(key);
            }
            else
                return false;
        }

        delegate void OperationInvoker();
        class ChangesList : List<Change<OperationInvoker>> 
        { 
            public void Add(OperationInvoker doChange, OperationInvoker undoChange)
            {
                Change<OperationInvoker> change = new Change<OperationInvoker>();
                change.OldState = undoChange;
                change.NewState = doChange;
                base.Add(change);
            }            
        }

        ChangesList Enlist()
        {
			UndoRedoArea.AssertCommand();
			Command command = UndoRedoArea.CurrentArea.CurrentCommand;
			if (!command.IsEnlisted(this))
			{
				ChangesList changes = new ChangesList();
				command[this] = changes;
				return changes;
			}
			else
				return (ChangesList)command[this];
        }

        #region IUndoRedoMember Members

        void IUndoRedoMember.OnCommit(object change)
        {}

        void IUndoRedoMember.OnUndo(object change)
        {
            ChangesList changesList = (ChangesList)change;
            for (int i = changesList.Count - 1; i >= 0; i--)
                changesList[i].OldState();
        }

        void IUndoRedoMember.OnRedo(object change)
        {
			ChangesList changesList = (ChangesList)change;
			for (int i = 0; i <= changesList.Count - 1; i++)
				changesList[i].NewState();
        }

        #endregion
    }
}
