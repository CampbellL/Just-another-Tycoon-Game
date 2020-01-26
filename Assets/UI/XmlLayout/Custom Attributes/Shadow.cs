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

namespace UI.Xml.CustomAttributes
{
    public class ShadowAttribute : CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            var elementTransform = xmlElement.rectTransform;

            if (value.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                var _shadow = GetShadowComponent(elementTransform);
                if (_shadow != null) _shadow.enabled = false;

                return;
            }
            if (!elementAttributes.ContainsKey("shadow")) return;

            var shadowColor = elementAttributes["shadow"].ToColor(xmlElement.xmlLayoutInstance);

            var shadow = GetShadowComponent(elementTransform);
            if (shadow == null)
            {
                shadow = elementTransform.gameObject.AddComponent<Shadow>();
            }

            shadow.enabled = true;
            shadow.effectColor = shadowColor;

            if (elementAttributes.ContainsKey("shadowdistance"))
            {
                shadow.effectDistance = elementAttributes["shadowdistance"].ToVector2();
            }
        }
        
        private Shadow GetShadowComponent(Transform transform)
        {
            Outline outline = transform.GetComponent<Outline>();

            if (outline == null) return transform.GetComponent<Shadow>();            

            return transform.GetComponents<Shadow>().FirstOrDefault(t => t.GetInstanceID() != outline.GetInstanceID());
        }

        public override string ValueDataType
        {
            get
            {
                return "color";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "None";
            }
        }
    }

    public class ShadowDistanceAttribute : ShadowAttribute
    {
        public override string ValueDataType
        {
            get
            {
                return "xs:float";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "1";
            }
        }
    }
}
