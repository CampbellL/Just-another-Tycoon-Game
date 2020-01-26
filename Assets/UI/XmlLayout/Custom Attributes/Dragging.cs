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
    public class AllowDraggingAttribute: CustomXmlAttribute
    {
        public override bool UsesConvertMethod { get { return true; } }
        public override bool KeepOriginalTag { get { return true; } }
        public override eAttributeGroup AttributeGroup { get { return eAttributeGroup.Dragging; } }


        public override AttributeDictionary Convert(string value, AttributeDictionary attributes, XmlElement xmlElement)
        {
            var result = new AttributeDictionary();

            // if allowDragging has been set, then raycastTarget must also be true (unless specified otherwise by the user)
            if (value.ToBoolean() && !attributes.ContainsKey("raycastTarget"))
            {
                result.Add("raycastTarget", "true");                
            }            

            return result;
        }
    }
}
