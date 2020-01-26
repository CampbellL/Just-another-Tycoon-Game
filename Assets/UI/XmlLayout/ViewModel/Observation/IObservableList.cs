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
    public interface IObservableList
    {
        string guid { get; set; }

        event Action<int, object, string> itemChanged;
        event Action<object> itemAdded;
        event Action<object> itemRemoved;

        int Count { get; }

        string GetGUID(object item);

        void NotifyItemChanged(object item, string changedField = null);

        int IndexOf(object item);

        int GetIndexByGUID(string guid);
        object GetItemByGUID(string guid);

        object this[int index]
        {
            get;
            set;
        }

        List<object> GetItems();

        Type itemType { get; }
    }
}
#endif
