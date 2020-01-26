#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UI.Xml;

namespace UI.Xml.Examples
{
    class DataTableExampleViewModel : XmlLayoutViewModel
    {
        public ObservableList<DataTableExampleListItem> myData { get; set; }
        public ObservableList<Dictionary<string, string>> myData2 { get; set; }
    }

    class DataTableExampleListItem : ObservableListItem
    {
        public DataTableExampleListItem(string c1, string c2, string c3, string c4)
        {
            col1 = c1;
            col2 = c2;
            col3 = c3;
            col4 = c4;
        }

        public string col1 { get; set; }
        public string col2 { get; set; }
        public string col3 { get; set; }
        public string col4 { get; set; }
    }
}
#endif
