#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;
using System.Linq;
using UnityEngine.UI;

namespace UI.Xml.Examples
{
    /// <summary>
    /// This is an example which utilises the new MVVM functionality.
    /// Specifically, this ViewModel provides a list of items, from which XmlLayout will create elements in the view.
    /// </summary>
    public class MVVMExampleListController : XmlLayoutController<MVVMExampleListViewModel>
    {
        /// <summary>
        /// This method will be called by XmlLayout to load the 'initial' view data.
        /// It will only be called once (although you can call it again in your own code if you wish)
        /// </summary>
        protected override void PrepopulateViewModelData()
        {
            // Add a few sample items to our list
            viewModel.listItems = new ObservableList<ExampleListItem>()
            {
                new ExampleListItem { column1 = 1, column2 = 4 },
                new ExampleListItem { column1 = 2, column2 = 5 },
                new ExampleListItem { column1 = 3, column2 = 6 },
            };

            // Please note: any lists that you do not populate here should at the very least be instantiated;
            // failing to do so may result in null exceptions
        }

        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever the 'Add Element' button is clicked
        /// </summary>
        void AddElement()
        {
            // Add a new item to the list
            viewModel.listItems.Add(new ExampleListItem { column1 = 5, column2 = 5 });

            // You may also insert, or add ranges of items, e.g:

            //viewModel.listItems.Insert(0, new ExampleListItem { column1 = "Ins", column2 = "erted" });
            //viewModel.listItems.AddRange(new List<ExampleListItem>() { new ExampleListItem { }, new ExampleListItem { } });
        }


        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever the 'Change Last' button is clicked
        /// </summary>
        void ChangeLast()
        {
            // Find the last item in the list via LINQ
            var item = viewModel.listItems.LastOrDefault();

            if (item != null)
            {
                // change a value
                item.column1 = 9;
            }
        }

        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever the 'X' button in a list item is clicked
        /// The method arguments for MVVM list items are as follows:
        /// {listName.item}  => The item itself (as shown here)
        /// {listName.index} => The index of the item in the list (integer)
        /// {listName.guid}  => The GUID of the list item (string)
        /// </summary>
        void Remove(ExampleListItem item)
        {
            // Remove the specified item from the list
            viewModel.listItems.Remove(item);
        }
    }
}
#endif
