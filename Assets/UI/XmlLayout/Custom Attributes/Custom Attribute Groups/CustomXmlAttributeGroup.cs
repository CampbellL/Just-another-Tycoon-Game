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
    public abstract class CustomXmlAttributeGroup
    {
        /// <summary>
        /// Override this to create your own custom group name.
        /// If left by default, the name will be the class name with 'AttributeGroup' removed, and the first character set to lower case
        /// </summary>
        public virtual string GroupName
        {
            get
            {
                var name = new StringBuilder(this.GetType().Name);
                name.Replace("AttributeGroup", String.Empty);
                name[0] = char.ToLower(name[0]);

                return name.ToString();
            }
        }

        /// <summary>
        /// Use this property to define which attributes to include in this group
        /// </summary>
        public abstract List<Type> CustomXmlAttributes
        {
            get;            
        }

        public bool Validate()
        {
            if (CustomXmlAttributes == null || !CustomXmlAttributes.Any())
            {
                Debug.LogWarning("[XmlLayout][CustomXmlAttributeGroup] Warning: a Custom Xml Attribute Group has no attributes defined.");
                return false;
            }

            var type = typeof(CustomXmlAttribute);

            if (CustomXmlAttributes.Any(t => !t.IsSubclassOf(type)))
            {
                Debug.LogWarning("[XmlLAyout][CustomXmlAttributeGroup] Warning: All Custom Xml Attributes must extend the CustomXmlAttribute class.");
                return false;
            }

            return true;
        }
    }
}
