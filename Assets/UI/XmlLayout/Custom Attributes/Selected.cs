using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Xml.CustomAttributes
{        
    public class SelectedAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            if (value.ToBoolean())
            {
                var selectable = xmlElement.GetComponent<Selectable>();
                if (selectable != null)
                {                    
                    XmlLayoutTimer.AtEndOfFrame(() => selectable.Select(), xmlElement);
                }
            }
        }

        public override string ValueDataType
        {
            get
            {
                return "xs:boolean";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "false";
            }
        }
    }
}
