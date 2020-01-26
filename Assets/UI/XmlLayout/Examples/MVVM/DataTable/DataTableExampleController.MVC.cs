#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UI.Xml;

namespace UI.Xml.Examples
{
    partial class DataTableExampleController //: XmlLayoutController
    {
        private List<Dictionary<string, string>> myData3 = new List<Dictionary<string, string>>()
        {
            new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
            new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
            new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
            new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
            //new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
            //new Dictionary<String, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
        };

        private XmlElementReference<XmlLayoutDataTable> dataTable3 = null;

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            // populate dataTable3 if necessary (this should only be once ever)
            if (dataTable3 == null) dataTable3 = XmlElementReference<XmlLayoutDataTable>("dataTable3");

            // Set the data of the dataTable directly
            dataTable3.element.SetData(myData3);
        }

        void MVC_AddItem()
        {
            // Add an item to our data collection
            myData3.Add(new Dictionary<string, string>() { { "A", "New" }, { "B", "New" }, { "C", "New" } });

            // Set the data of the dataTable directly
            dataTable3.element.SetData(myData3);
        }

        void MVC_RemoveLast()
        {
            if (myData3.Any()) myData3.RemoveAt(myData3.Count - 1);

            dataTable3.element.SetData(myData3);
        }

        void MVC_ChangeLast()
        {
            if (myData3.Any()) myData3.Last()["B"] = "+++";

            dataTable3.element.SetData(myData3);
        }

        void MVC_ReplaceLast()
        {
            if (myData3.Any()) myData3[myData3.Count - 1] = new Dictionary<string, string>() { { "A", "***" }, { "B", "***" }, { "C", "***" } };

            dataTable3.element.SetData(myData3);
        }
    }
}
#endif
