using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Tags
{
    public class ProgressBarTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlLayoutProgressBar>();
            }
        }

        public override bool isCustomElement
        {
            get { return true; }
        }

        public override string elementChildType
        {
            get { return "none"; }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"percentage", "xs:float"},
                    {"showPercentageText", "xs:boolean"},
                    {"percentageTextFormat", "xs:string"},
                    {"textShadow", "xmlLayout:color"},
                    {"textOutline", "xmlLayout:color"},
                    {"textColor", "xmlLayout:color"},
                    {"textAlignment", String.Join(",", Enum.GetNames(typeof(TextAnchor)))},
                    {"fillImage", "xs:string"},
                    {"fillImageColor", "xmlLayout:color"},
                    {"horizontal", "xs:boolean" },
                    {"reverse", "xs:boolean" }
                };
            }
        }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "text",
                    "image",
                };
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            base.ApplyAttributes(attributesToApply);

            var progressBar = primaryComponent as XmlLayoutProgressBar;

            var textComponent = progressBar.ref_text;

            var tagHandler = XmlLayoutUtilities.GetXmlTagHandler("Text");
            tagHandler.SetInstance(textComponent.rectTransform, this.currentXmlLayoutInstance);

            var textAttributes = new AttributeDictionary(
                                            attributesToApply.Where(a => TextTagHandler.TextAttributes.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
                                                      .ToDictionary(a => a.Key, b => b.Value));

            if (attributesToApply.ContainsKey("textshadow")) textAttributes.Add("shadow", attributesToApply["textshadow"]);
            if (attributesToApply.ContainsKey("textoutline")) textAttributes.Add("outline", attributesToApply["textoutline"]);
            if (attributesToApply.ContainsKey("textcolor")) textAttributes.Add("color", attributesToApply["textcolor"]);
            if (attributesToApply.ContainsKey("textalignment")) textAttributes.Add("alignment", attributesToApply["textalignment"]);

            tagHandler.ApplyAttributes(textAttributes);

            var fillImage = progressBar.ref_fillImage;

            if (attributesToApply.ContainsKey("fillImage")) fillImage.sprite = attributesToApply.GetValue<Sprite>("fillImage");
            if (attributesToApply.ContainsKey("fillImageColor")) fillImage.color = attributesToApply["fillImageColor"].ToColor(currentXmlLayoutInstance);
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            var v = float.Parse(newValue);

            (primaryComponent as XmlLayoutProgressBar).percentage = v;
        }
    }
}
