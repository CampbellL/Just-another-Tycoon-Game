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
    public class CursorAttribute : CustomXmlAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            xmlElement.cursor = value.ToCursorInfo();
        }
    }
    
    public class CursorClickAttribute : CursorAttribute 
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            xmlElement.cursorClick = value.ToCursorInfo();
        }
    }
}
