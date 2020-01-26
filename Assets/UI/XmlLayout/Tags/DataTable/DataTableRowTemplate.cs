using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Tags
{
    public class DataTableRowTemplateTagHandler : ElementTagHandler
    {
        internal static string currentTemplateType { get; private set; }

        public override string prefabPath
        {
            get
            {
                return null;
            }
        }

        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<TableRow>();
            }
        }

        public override bool isCustomElement { get { return true; } }
        public override string elementChildType { get { return "dataTableRow"; } }
        public override string elementGroup { get { return "dataTable"; } }

        public override string extension
        {
            get
            {
                return "simple";
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"preferredHeight", "xs:float"},
                    {"dontUseTableRowBackground", "xs:boolean"},
                    {"templateType", "HeaderRow,DataRow"}
                };
            }
        }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "image"
                };
            }
        }

        public override bool renderElement
        {
            get
            {
                return false;
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributes)
        {            
            if (!attributes.ContainsKey("templateType"))
            {
                Debug.LogWarningFormat("[XmlLayout][DataTableRowTemplate] The 'templateType' attribute is required.");
                return;
            }

            var templateType = attributes["templateType"];
            var dataTable = currentXmlElement.GetComponentInParent<XmlLayoutDataTable>();

            List<TableRow> rows = new List<TableRow>();
            
            switch (templateType)
            {
                case "HeaderRow":
                    rows.Add(dataTable.templateHeaderRow);
                    rows.Add(dataTable.headerRow);
                    break;
                case "DataRow":
                    rows.Add(dataTable.templateDataRow);
                    rows.AddRange(dataTable.dataRows);
                    break;
            }

            rows = rows.Where(r => r != null).ToList();

            if (rows.Count > 0)
            {
                var rowTagHandler = XmlLayoutUtilities.GetXmlTagHandler("Row");

                foreach (var row in rows)
                {
                    var rowXmlElement = row.GetComponent<XmlElement>();
                    if (rowXmlElement == null)
                    {
                        rowXmlElement = row.gameObject.AddComponent<XmlElement>();
                        rowXmlElement.Initialise(currentXmlElement.xmlLayoutInstance, row.transform as RectTransform, rowTagHandler);
                    }

                    rowXmlElement.ApplyAttributes(attributes);   
                }
            }         
        }

        public override void Open(AttributeDictionary attributes)
        {
            base.Open(attributes);

            if (attributes.ContainsKey("templateType"))
            {
                currentTemplateType = attributes["templateType"];
            }
        }        
    }
}
