#if UNITY_2018_1_OR_NEWER
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace UI.Xml.Tags
{
    [ElementTagHandler("SVGImage")]
    public class SVGImageTagHandler : ElementTagHandler
    {
        public override bool isCustomElement { get { return true; } }
        public override string prefabPath { get { return null; } }

        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<XmlLayoutSVGImage>();
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "image", "xs:string" },
                    { "preserveAspect", "xs:boolean" },
                };
            }
        }

        public override XmlElement GetInstance(RectTransform parent, XmlLayout xmlLayout, string overridePrefabPath = null)
        {
            var instance = base.GetInstance(parent, xmlLayout, overridePrefabPath);

            instance.gameObject.name = "SVGImage";
            instance.gameObject.AddComponent<XmlLayoutSVGImage>();
            instance.gameObject.AddComponent<LayoutElement>();

            MatchParentDimensions();

            return instance;
        }
    }
}
#endif