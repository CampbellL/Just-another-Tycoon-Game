#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UI.Xml;

namespace UI.Xml.Examples.MVVM.FilteredList
{
    public class MVVMExampleFilteredListController : XmlLayoutController<MVVMExampleFilteredListViewModel>
    {
        protected override void PrepopulateViewModelData()
        {
            viewModel.items = new ObservableList<MVVMExampleFilteredListControllerItem>()
            {
                new MVVMExampleFilteredListControllerItem { name = "Item One", selected = true },
                new MVVMExampleFilteredListControllerItem { name = "Item Two", selected = true },
                new MVVMExampleFilteredListControllerItem { name = "Item Three", selected = true },
                new MVVMExampleFilteredListControllerItem { name = "Item Four", selected = true },
                new MVVMExampleFilteredListControllerItem { name = "Item Five", selected = true },
                new MVVMExampleFilteredListControllerItem { name = "Item Six", selected = false },
                new MVVMExampleFilteredListControllerItem { name = "Item Seven", selected = false },
                new MVVMExampleFilteredListControllerItem { name = "Item Eight", selected = false },
                new MVVMExampleFilteredListControllerItem { name = "Item Nine", selected = false },
                new MVVMExampleFilteredListControllerItem { name = "Item Ten", selected = false },
            };
        }
    }
}
#endif
