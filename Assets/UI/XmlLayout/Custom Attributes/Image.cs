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
    public class ImageAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        private void ApplyLocalValue(XmlElement xmlElement, string value)
        {
            var image = xmlElement.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = value.ToSprite();
            }
        }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var image = xmlElement.GetComponent<Image>();

            if (!xmlElement.HasAttribute("color"))
            {
                image.color = Color.white;
            }

            if (string.IsNullOrEmpty(value) || value == "none")
            {
                image.sprite = null;
                return;
            }

            Sprite entry = XmlLayoutResourceDatabase.instance.GetResource<Sprite>(value);
            if (entry == null)
            {
                if (value.IsUrl())
                {
                    XmlLayoutResourceDatabase.instance.LoadResourcesFromUrls<Sprite>(
                        () =>
                        {
                            ApplyLocalValue(xmlElement, value);
                        },
                        value);
                }
                else
                {
                    Debug.LogError("[XmlLayout] Unable to load sprite '" + value + "'. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
                }
            }
            else
            {
                ApplyLocalValue(xmlElement, value);
            }
        }

        public override eAttributeGroup AttributeGroup { get { return eAttributeGroup.Image; } }
    }
}
