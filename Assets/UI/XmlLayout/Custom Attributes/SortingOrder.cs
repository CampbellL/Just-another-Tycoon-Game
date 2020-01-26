using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml.CustomAttributes
{
    public class SortingOrderAttribute : CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override bool RestrictToPermittedElementsOnly { get { return true; } }

        public override List<string> PermittedElements { get { return new List<string>() { "Canvas" }; } }

        public override string ValueDataType { get { return "xs:int"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var canvas = xmlElement.GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = xmlElement.gameObject.AddComponent<Canvas>();
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            var sortingOrder = value.ToInt();

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
    }    
}
