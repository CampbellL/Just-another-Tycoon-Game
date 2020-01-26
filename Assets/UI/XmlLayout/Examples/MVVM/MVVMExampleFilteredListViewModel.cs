#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UI.Xml;

namespace UI.Xml.Examples.MVVM.FilteredList
{
    public class MVVMExampleFilteredListViewModel : XmlLayoutViewModel
    {
        public ObservableList<MVVMExampleFilteredListControllerItem> items { get; set; }

        public ObservableList<MVVMExampleFilteredListControllerItem> ownedItems
        {
            get
            {
                return items.Where(i => i.selected).ToObservableList();
            }
        }

        public ObservableList<MVVMExampleFilteredListControllerItem> unownedItems
        {
            get
            {
                return items.Where(i => !i.selected).ToObservableList();
            }
        }
    }

    public class MVVMExampleFilteredListControllerItem
    {
        public string name { get; set; }
        public bool selected { get; set; }
    }
}
#endif
