#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;

class MVVMExampleToggleGroupController : XmlLayoutController<MVVMExampleToggleGroupViewModel>
{
    protected override void PrepopulateViewModelData()
    {
        viewModel.toggleItems = new ObservableList<MVVMExampleToggleGroupViewModel.ToggleItem>()
        {
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item One", textColor = "rgb(1,1,1)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Two", textColor = "rgb(1,1,0)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Three", textColor = "rgb(1,0,1)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Four", textColor = "rgb(1,0,0)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Five", textColor = "rgb(0.5,0.5,0.5)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Six", textColor = "rgb(0,0,1)"},
            new MVVMExampleToggleGroupViewModel.ToggleItem { text = "Item Seven", textColor = "rgb(0,1,1)"},
        };

        viewModel.toggleValue = "Item Five";
    }

    public override void ViewModelMemberChanged(string memberName)
    {
        if (memberName == "toggleValue")
        {
            Debug.Log("New value: " + viewModel.toggleValue);
        }
    }
}

class MVVMExampleToggleGroupViewModel : XmlLayoutViewModel
{
    public string toggleValue { get; set; }
    public ObservableList<ToggleItem> toggleItems { get; set; }

    public class ToggleItem : ObservableListItem
    {
        public string text { get; set; }
        public string textColor { get; set; }
    }
}
#endif