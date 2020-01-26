/*
 * 'CustomXmlAttribute' - Extend this class to create custom attributes to use in XmlLayout.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml
{
    public abstract class CustomXmlAttribute
    {
        /// <summary>
        /// You should override this value if your CustomXmlAttribute uses the Convert() method
        /// </summary>
        public virtual bool UsesConvertMethod {  get { return false; } }

        public virtual AttributeDictionary Convert(string value, AttributeDictionary attributes, XmlElement xmlElement)
        {
            return null;
        }

        /// <summary>
        /// You should override this value if your CustomXmlAttribute uses the Apply() method
        /// </summary>
        public virtual bool UsesApplyMethod { get { return false; } }

        public virtual void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
        }

        /// <summary>
        /// If this is set to true, then only elements which are contained within 'PermittedElements' will be affected by this CustomXmlAttribute
        /// </summary>
        public virtual bool RestrictToPermittedElementsOnly { get { return false; } }
        public virtual List<string> PermittedElements { get { return new List<string>(); } }

        /// <summary>
        /// If this is set to false, then XmlLayout will remove the original custom tag from the attribute list once it has been processed by its CustomXmlAttribute.Convert
        /// </summary>
        public virtual bool KeepOriginalTag { get { return false; } }

        /// <summary>
        /// Must be a valid datatype within XmlLayout.xsd or http://www.w3.org/2001/XMLSchema
        /// e.g.
        /// xs:string -> string value (from http://www.w3.org/2001/XMLSchema)
        /// xs:integer -> integer value (from http://www.w3.org/2001/XMLSchema)
        /// xs:float -> float value (from http://www.w3.org/2001/XMLSchema)
        /// xmlLayout:color -> color in hex/rgb/rgba format
        /// xmlLayout:vector2 -> vector 2 in x y format
        /// xmlLayout:floatList -> list of floats e.g. "10 10 10"
        /// </summary>
        public virtual string ValueDataType { get { return "xs:string"; } }

        public enum eAttributeGroup
        {
            AllElements,                            // Applied to all elements
            RectTransform,                          // Applied to any element with a RectTransform [ standard default RectTransform properties ]
            RectPosition,                           // Applied to any element with a RectTransform [ XmlLayout-specific RectTransform properties e.g. rectAlignment ]
            LayoutElement,                          // Applied to any element with a LayoutElement component
            LayoutBase,                             // Applied to layout group elements
            Image,                                  // Applied to any element with an Image component
            Text,                                   // Applied to any element with a text component (e.g. Text, Button, etc.)
            Animation,                              // Applied to most elements
            Events,                                 // Applied to most elements
            Button,                                 // Applied to Button and ToggleButton elements
            Dragging,                               // Applied to most elements
            Custom,                                 // Special handling; this attribute will be part of a CustomXmlAttributeGroup
            Tooltip
        }

        public virtual eAttributeGroup AttributeGroup { get { return eAttributeGroup.AllElements; } }

        /// <summary>
        /// Get the default value for this attribute.
        /// </summary>
        /// <returns></returns>
        public virtual string DefaultValue
        {
            get
            {
                return String.Empty;
            }
        }
    }
}
