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
    public static class ObservableListExtensions
    {
        public static ObservableList<T> ToObservableList<T>(this IEnumerable<T> collection)
            where T : class
        {
            var list = new ObservableList<T>();

            if(collection != null) list.AddRange(collection);

            return list;
        }
    }
}
#endif
