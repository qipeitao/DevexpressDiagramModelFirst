using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace DevexpressDiagramTest
{
    public class ListItemCollection<T> : IList<T>, IList
    {
        private readonly ObservableCollection<T> _headContents = new ObservableCollection<T>();
        public ListItemCollection()
        {
            _headContents.CollectionChanged += _headContents_CollectionChanged;
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void _headContents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _headContents.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Add(T item)
        {
            _headContents.Add(item);
        }
        public int Add(object value)
        {
            Add((T)value);
            return _headContents.Count;
        }
        public bool Contains(object value)
        {
            return _headContents.Contains((T)value);
        }
        public void Clear()
        {
            _headContents.Clear();
        }
        public int IndexOf(object value)
        {
            return _headContents.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            _headContents.Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            _headContents.Remove((T)value);
        }

        public void RemoveAt(int index)
        {
            var v = _headContents[index];
            _headContents.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get => _headContents[index];
            set => _headContents[index] = (T)value;
        }

        public bool Contains(T item)
        {
            return _headContents.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _headContents.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var s = _headContents.Remove(item);
            return s;
        }

        public void CopyTo(Array array, int index)
        {
            _headContents.CopyTo((T[])array, index);
        }

        public int Count => _headContents.Count;

        public object SyncRoot { get; }

        public bool IsSynchronized { get; }

        public bool IsReadOnly { get; }

        public bool IsFixedSize { get; }

        public int IndexOf(T item)
        {
            return _headContents.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _headContents.Insert(index, item);
        }
        
        public T this[int index]
        {
            get => _headContents[index];
            set => _headContents[index] = value;
        }
    }
}
