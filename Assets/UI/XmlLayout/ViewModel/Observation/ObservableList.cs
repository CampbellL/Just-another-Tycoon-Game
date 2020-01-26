#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;

namespace UI.Xml
{
    /// <summary>
    /// This is a special type of List used by XmlLayout which keeps track of changes to its elements,
    /// and notifies XmlLayout so that, for example, the view can be updated automatically when
    /// changes are made to the view model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ObservableList<T> : List<T>, IObservableList, IList<T>
        where T: class
    {
        public string guid { get; set; }

        public event Action<int, object, string> itemChanged = delegate { };
        public event Action<object> itemAdded = delegate { };
        public event Action<object> itemRemoved = delegate { };

        public Dictionary<string, T> itemGuids = new Dictionary<string, T>();

        public ObservableList()
        {
            guid = Guid.NewGuid().ToString();
        }

        public new void Add(T item)
        {
            base.Add(item);

            AddGUID(item);

            itemAdded(item);
        }

        public new void Remove(T item)
        {
            itemRemoved(item);

            base.Remove(item);

            RemoveGUID(item);
        }

        public new void AddRange(IEnumerable<T> items)
        {
            var itemList = items.ToList();

            base.AddRange(itemList);

            foreach (var item in itemList)
            {
                AddGUID(item);
            }

            itemList.ForEach(item => itemAdded(item));
        }

        public void ReplaceItems(IEnumerable<T> items)
        {
            Clear();
            AddRange(items);
        }

        public new void RemoveRange(int index, int count)
        {
            List<T> itemsRemoved = new List<T>();
            for (var i = index; i < index + count; i++)
            {
                itemsRemoved.Add(this[i]);
            }

            base.RemoveRange(index, count);

            itemsRemoved.ForEach(item => itemRemoved(item));

            foreach (var item in itemsRemoved)
            {
                RemoveGUID(item);
            }
        }

        public new void Clear()
        {
            var itemsRemoved = this.ToList();

            base.Clear();

            itemsRemoved.ForEach(item => itemRemoved(item));

            foreach (var item in itemsRemoved)
            {
                RemoveGUID(item);
            }
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);

            AddGUID(item);

            itemAdded(item);
        }

        public new void InsertRange(int index, IEnumerable<T> items)
        {
            base.InsertRange(index, items);

            foreach (var item in items)
            {
                itemAdded(item);
            }
        }

        public new void RemoveAll(Predicate<T> match)
        {
            var itemsRemoved = this.Where(item => match(item)).ToList();

            base.RemoveAll(match);

            itemsRemoved.ForEach(item => itemRemoved(item));

            foreach (var item in itemsRemoved)
            {
                RemoveGUID(item);
            }
        }

        public new void RemoveAt(int index)
        {
            var item = this[index];

            base.RemoveAt(index);

            itemRemoved(item);

            RemoveGUID(item);
        }

        public List<object> GetItems()
        {
            return this.Select(i => (object)i).ToList();
        }

        public new T this[int index]
        {
            get
            {
                if (base[index] is ObservableListItem)
                {
                    return ObservableListItemProxy<T>.Create(base[index], this);
                }
                else
                {
                    return base[index];
                }
            }
            set
            {
                var guid = GetGUID(base[index]);

                base[index] = value;

                itemGuids[guid] = value;

                itemChanged(index, value, null);
            }
        }

        object IObservableList.this[int index]
        {
            get
            {
                if (base[index] is ObservableListItem || base[index] is Dictionary<string,string>)
                {
                    return ObservableListItemProxy<T>.Create(base[index], this);
                }
                else
                {
                    return base[index];
                }
            }
            set
            {
                var guid = GetGUID(base[index]);

                base[index] = (T)value;

                itemGuids[guid] = (T)value;

                itemChanged(index, value, null);
            }
        }

        private void AddGUID(T item)
        {
            itemGuids.Add(Guid.NewGuid().ToString(), item);
        }

        private void RemoveGUID(T item)
        {
            if(itemGuids.Any(f => f.Value.Equals(item)))
            {
                itemGuids.Remove(itemGuids.First(f => f.Value.Equals(item)).Key);
            }
        }

        public string GetGUID(object item)
        {
            if (!(item is T)) return null;

            var castItem = (T)item;

            var guid = itemGuids.FirstOrDefault(f => f.Value.Equals(castItem));

            return guid.Key;
        }

        public int GetIndexByGUID(string guid)
        {
            if (!itemGuids.ContainsKey(guid)) return -1;

            var item = itemGuids[guid];

            return base.IndexOf(item);
        }

        public object GetItemByGUID(string guid)
        {
            if (!itemGuids.ContainsKey(guid)) return -1;

            var index = GetIndexByGUID(guid);

            return this[index];
        }

        public void NotifyItemChanged(object item, string changedItem = null)
        {
            var value = (T)item;

            var index = base.IndexOf(value);

            itemChanged(index, value, changedItem);
        }

        public int IndexOf(object item)
        {
            var value = (T)item;
            return base.IndexOf(value);
        }

        private Type _itemType = null;
        public Type itemType
        {
            get
            {
                if (_itemType == null) _itemType = typeof(T);

                return _itemType;
            }
        }
    }
}
#endif
