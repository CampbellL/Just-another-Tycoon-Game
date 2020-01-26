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
    /// This is an example of a ViewModel which can be used by XmlLayout's new MVVM functionality
    /// </summary>
    public class MVVMExampleListViewModel : XmlLayoutViewModel
    {
        /// <summary>
        /// This is a list of items which will be rendered by XmlLayout
        /// </summary>
        public ObservableList<ExampleListItem> listItems { get; set; }
    }

    /// <summary>
    /// Items stored in an Observable list should always extend 'ObservableListItem',
    /// so that their fields and properties can be monitored by XmlLayout
    /// </summary>
    public class ExampleListItem : ObservableListItem
    {
        // These properties will be monitored for changes by XmlLayout and replicated to the view
        public int column1 { get; set; }
        public int column2 { get; set; }

        // This property will also be replicated to the view
        public int combined
        {
            get
            {
                return column1 + column2;
            }
        }
    }
}
#endif
