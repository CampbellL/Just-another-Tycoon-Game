
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace UI.Xml.Tags
{
    [ElementTagHandler("AccordionSectionContent")]
    public class AccordionSectionContentTagHandler : ElementTagHandler
    {        
        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return "XmlLayout Prefabs/Accordion/AccordionSectionContent"; } }

        public override string elementGroup { get { return "AccordionSection"; } }

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

        public override RectTransform transformToAddChildrenTo
        {
            get
            {
                return currentInstanceTransform.GetChild(0) as RectTransform;
            }
        }

        public override void Close()
        {           
            var section = currentInstanceTransform.GetComponentInParent<XmlLayoutAccordionSection>();
            var transformTemp = transformToAddChildrenTo;

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                transformTemp.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, section.contentHeight);

                if (transformTemp.childCount > 0)
                {
                    var firstChild = transformTemp.GetChild(0);
                    var xmlElement = firstChild.GetComponent<XmlElement>();

                    if (xmlElement != null && xmlElement.tagType.EndsWith("ScrollView"))
                    {
                        xmlElement.ApplyAttributes(new AttributeDictionary()
                        {
                            { "width", "100%" },
                            { "height", "100%" }
                        });
                    }
                    
                }
            }, section);
        }
    }
}

