using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Tags
{
    public class DataTableCellTemplateTagHandler : ElementTagHandler
    {
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
                return currentInstanceTransform.GetComponent<TableCell>();
            }
        }

        public override bool isCustomElement { get { return true; } }
        public override string elementChildType { get { return null; } }
        public override string elementGroup
        {
            get
            {
                return "dataTableRow";
            }
        }

        public override string extension
        {
            get
            {
                return "base";
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {            
                    {"dontUseTableCellBackground", "xs:boolean"},
                    {"columnSpan", "xs:int"},
                    {"overrideGlobalPadding", "xs:boolean"}
                };
            }
        }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "image",
                    "layoutBase"
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
            TableCell cellTemplate = null;
            switch (DataTableRowTemplateTagHandler.currentTemplateType)
            {
                case "HeaderRow":
                    cellTemplate = DataTableTagHandler.currentDataTable.templateHeaderCell;
                    break;

                case "DataRow":
                    cellTemplate = DataTableTagHandler.currentDataTable.templateDataCell;
                    break;
            }

            if (cellTemplate != null)
            {
                var cellTagHandler = XmlLayoutUtilities.GetXmlTagHandler("Cell");

                var cellXmlElement = cellTemplate.GetComponent<XmlElement>();
                if (cellXmlElement == null)
                {
                    cellXmlElement = cellTemplate.gameObject.AddComponent<XmlElement>();
                    cellXmlElement.Initialise(currentXmlElement.xmlLayoutInstance, cellTemplate.transform as RectTransform, cellTagHandler);
                }

                cellXmlElement.ApplyAttributes(attributes);
            }                        
        }
    }
}
