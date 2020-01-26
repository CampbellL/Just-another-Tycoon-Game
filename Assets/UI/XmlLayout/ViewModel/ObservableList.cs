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
    {
        // Implementation in Observation/ObservableList
        // Left this file here as I've noticed that importing a UnityPackage doesn't always remove files that were deleted
    }
}
#endif
