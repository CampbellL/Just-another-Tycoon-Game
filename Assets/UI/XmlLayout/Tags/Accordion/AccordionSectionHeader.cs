
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;


namespace UI.Xml.Tags
{
    [ElementTagHandler("AccordionSectionHeader")]
    public class AccordionSectionHeaderTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<LayoutGroup>();
            }
        }

        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return "XmlLayout Prefabs/Accordion/AccordionSectionHeader"; } }

        public override string elementGroup { get { return "AccordionSection"; } }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "image",                    
                };                
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {                
                return new Dictionary<string, string>()
                {
                    { "padding", "xmlLayout:rectOffset" },
                    { "spacing", "xs:float" },
                    { "childAlignment", String.Join(",", Enum.GetNames(typeof(TextAnchor))) },
                };
            }
        }
    }
}

