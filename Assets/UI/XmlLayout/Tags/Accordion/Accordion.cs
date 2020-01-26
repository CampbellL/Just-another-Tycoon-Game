
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace UI.Xml.Tags
{
    [ElementTagHandler("Accordion")]
    public class AccordionTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlLayoutAccordion>();
            }
        }

        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return "XmlLayout Prefabs/Accordion/Accordion"; } }

        public override string elementChildType { get { return "Accordion"; } }

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
                    { "collapsible", "xs:boolean" },
                    { "accordionAnimationDuration", "xs:float" },
                    { "allowSectionReordering", "xs:boolean" },
                    { "expandSectionAfterReorder", "xs:boolean" },
                    { "makeDraggedSectionTransparent", "xs:boolean" }
                };
            }
        }

        public override void Close()
        {
            var allowSectionReordering = currentXmlElement.GetAttribute("allowSectionReordering").ToBoolean();
            
            var accordion = primaryComponent as XmlLayoutAccordion;

            AttributeDictionary attributesToApply = null;            

            if (allowSectionReordering)
            {
                attributesToApply = new AttributeDictionary()
                {
                    { "allowDragging", "true" },
                    { "restrictDraggingToParentBounds", "true" },
                };
            }
            else
            {
                attributesToApply = new AttributeDictionary()
                {
                    { "allowDragging", "false" },
                };
            }

            foreach (var section in accordion.sections)
            {
                section.xmlElement.ApplyAttributes(attributesToApply);
            }            
        }
    }
}

