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
    public class SetNativeSizeAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            var _xmlElement = xmlElement;

            if (!Application.isPlaying)
            {
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    var size = _xmlElement.GetComponent<UnityEngine.UI.Image>().sprite.rect;
                    _xmlElement.SetAttribute("width", size.width.ToString());
                    _xmlElement.SetAttribute("height", size.height.ToString());
                    _xmlElement.ApplyAttributes();
                }, _xmlElement);
            }
            else
            {
                _xmlElement.ExecuteNowOrWhenElementIsEnabled(() =>
                {
                    XmlLayoutTimer.AtEndOfFrame(() =>
                    {
                        if (_xmlElement == null) return;

                        var size = _xmlElement.GetComponent<UnityEngine.UI.Image>().sprite.rect;
                        _xmlElement.SetAttribute("width", size.width.ToString());
                        _xmlElement.SetAttribute("height", size.height.ToString());
                        _xmlElement.ApplyAttributes();
                    }, null, true);
                });                
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

        public override eAttributeGroup AttributeGroup
        {
            get
            {
                return eAttributeGroup.Image;
            }
        }
    }
}
