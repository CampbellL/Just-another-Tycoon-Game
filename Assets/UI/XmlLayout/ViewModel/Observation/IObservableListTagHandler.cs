#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Xml
{
    interface IObservableListTagHandler
    {
        bool IsHandlingList(IObservableList list);

        void RemoveListItem(IObservableList list, object item, string listName);
        void AddListItem(IObservableList list, object item, string listName);
        void UpdateListItem(IObservableList list, int index, object item, string listName, string changedField = null);
    }
}
#endif
