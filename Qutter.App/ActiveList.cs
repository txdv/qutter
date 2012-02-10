using System;
using System.Collections.Generic;

namespace Qutter.App
{
	public interface IActiveList<T1, T2> : IEnumerable<T2>
	{
		int Current { get; set; }
		int Count { get; }

		void Add(T1 key, T2 value);
		void Remove(T1 key);

		event Action<T1, T2> AddItem;
		event Action<T1> RemoveItem;
		event Action<int> CurrentChanged;

		KeyValuePair<T1, T2> GetKeyValuePair(int index);
		int IndexOfKey(T1 key);
		int IndexOfValue(T2 value);
	}

	public class ActiveList<T1, T2> : IActiveList<T1, T2>
	{
		SortedList<T1, T2> sortedlist = new SortedList<T1, T2>();

		public ActiveList()
		{
		}

		int current = 0;
		public int Current {
			get {
				return current;
			}
			set {
				if (current == value) {
					return;
				} else if (Count == 0) {
					return;
				}
				current = value;
				if (current < 0) {
					current = Count - 1;
				} else if (current >= Count) {
					current = 0;
				}
				OnCurrentChanged(current);
			}
		}

		public int Count {
			get {
				return sortedlist.Count;
			}
		}

		public void Add(T1 key, T2 item)
		{
			sortedlist.Add(key, item);
			OnAddItem(key, item);
		}

		public void Remove(T1 key)
		{
			sortedlist.Remove(key);
			OnRemoveItem(key);
		}

		protected void OnCurrentChanged(int index)
		{
			if (CurrentChanged != null) {
				CurrentChanged(index);
			}
		}

		protected void OnAddItem(T1 key, T2 value)
		{
			if (AddItem != null) {
				AddItem(key, value);
			}
		}

		protected void OnRemoveItem(T1 key)
		{
			if (RemoveItem != null) {
				RemoveItem(key);
			}
		}

		public event Action<int> CurrentChanged;
		public event Action<T1, T2> AddItem;
		public event Action<T1> RemoveItem;

		public KeyValuePair<T1, T2> GetKeyValuePair(int index)
		{
			return new KeyValuePair<T1, T2>(sortedlist.Keys[index], sortedlist.Values[index]);
		}

		public int IndexOfKey(T1 key)
		{
			return sortedlist.IndexOfKey(key);
		}

		public int IndexOfValue(T2 value)
		{
			return sortedlist.IndexOfValue(value);
		}

		public IEnumerator<T2> GetEnumerator ()
		{
			return sortedlist.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator();
		}
	}

	public class ActiveListWrapper<T1, T2, T3> : IActiveList<T1, T3>
	{
		SortedList<T1, T3> sortedlist = new SortedList<T1, T3>();
		public IActiveList<T2, T3> Base { get; protected set; }

		public Func<T3, T1> KeyGenerator { get; protected set; }

		public ActiveListWrapper(ActiveList<T2, T3> list, Func<T2, T3, T1> keygenerator)
		{
			Base = list;

			Base.AddItem += (key, value) => {
				var k = keygenerator(key, value);
				Add (k, value);
				current = IndexOfValue(value);
			};

			Base.CurrentChanged += (index) => {
				var kvp = Base.GetKeyValuePair(index);
				var key = keygenerator(kvp.Key, kvp.Value);
				OnCurrentChanged(sortedlist.IndexOfKey(key));

			};
		}

		int current;
		public int Current {
			get {
				return current;
			}
			set {
				if (current == value) {
					return;
				} else if (Count == 0) {
					return;
				}
				current = value;
				if (current < 0) {
					current = Count - 1;
				} else if (current >= Count) {
					current = 0;
				}


				var buffer = GetKeyValuePair(current).Value;
				Base.Current = Base.IndexOfValue(buffer);
			}
		}

		public int Count {
			get {
				return Base.Count;
			}
		}

		public void Add(T1 key, T3 value)
		{
			sortedlist.Add(key, value);
			OnAddItem(key, value);
		}

		public void Remove(T1 key)
		{
			sortedlist.Remove(key);
			OnRemoveItem(key);
		}

		protected void OnCurrentChanged(int index)
		{
			if (CurrentChanged != null) {
				CurrentChanged(index);
			}
		}

		protected void OnAddItem(T1 key, T3 value)
		{
			if (AddItem != null) {
				AddItem(key, value);
			}
		}

		protected void OnRemoveItem(T1 key)
		{
			if (RemoveItem != null) {
				RemoveItem(key);
			}
		}

		public event Action<T1, T3> AddItem;
		public event Action<T1> RemoveItem;
		public event Action<int> CurrentChanged;

		public KeyValuePair<T1, T3> GetKeyValuePair (int index)
		{
			return new KeyValuePair<T1, T3>(sortedlist.Keys[index], sortedlist.Values[index]);
		}

		public int IndexOfKey(T1 key)
		{
			return sortedlist.IndexOfKey(key);
		}

		public int IndexOfValue(T3 value)
		{
			return sortedlist.IndexOfValue(value);
		}

		public IEnumerator<T3> GetEnumerator ()
		{
			return sortedlist.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator();
		}
	}
}

