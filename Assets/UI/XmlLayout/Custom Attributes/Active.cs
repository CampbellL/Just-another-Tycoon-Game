using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml.CustomAttributes
{    
    /// <summary>
    /// Controls the 'active' attribute for this element's GameObject.
    /// </summary>
    public class ActiveAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            var isActive = value.ToBoolean();

            xmlElement.gameObject.SetActive(isActive);            
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
                return "true";
            }
        }
    }
}
