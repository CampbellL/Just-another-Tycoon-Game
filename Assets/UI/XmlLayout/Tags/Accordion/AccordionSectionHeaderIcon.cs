
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace UI.Xml.Tags
{
    [ElementTagHandler("AccordionSectionHeaderIcon")]
    public class AccordionSectionHeaderIconTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<XmlLayoutAccordionSectionHeaderIcon>();
            }
        }

        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return "XmlLayout Prefabs/Accordion/AccordionSectionHeaderIcon"; } }                

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "image"
                };                
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "collapsedSprite", "xs:string" },
                    { "collapsedColor", "xmlLayout:color" },
                    { "expandedSprite", "xs:string" },
                    { "expandedColor", "xmlLayout:color" },
                };
            }
        }
    }
}

