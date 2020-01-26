
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;


namespace UI.Xml.Tags
{
    [ElementTagHandler("AccordionSection")]
    public class AccordionSectionTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlLayoutAccordionSection>();
            }
        }

        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return "XmlLayout Prefabs/Accordion/AccordionSection"; } }

        public override string elementGroup { get { return "Accordion"; } }

        public override string elementChildType { get { return "AccordionSection"; } }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "expanded", "xs:boolean" },
                    { "contentHeight", "xs:float" }
                };
            }
        }

        public override void Close()
        {
            var accordion = currentXmlElement.GetComponentInParent<XmlLayoutAccordion>();

            if (accordion == null) return;

            var section = primaryComponent as XmlLayoutAccordionSection;

            if (!accordion.sections.Contains(section))
            {
                accordion.sections.Add(section);
            }

            var header = currentXmlElement.childElements.FirstOrDefault(c => c.tagType == "AccordionSectionHeader");
            section.header = header.GetComponent<XmlLayoutAccordionSectionHeader>();

            section.header.xmlElement.AddOnClickEvent((ev) => accordion.SectionHeaderClicked(section));

            var content = currentXmlElement.childElements.FirstOrDefault(c => c.tagType == "AccordionSectionContent");
            section.content = content.GetComponent<XmlLayoutAccordionSectionContent>();
            content.rectTransform.pivot = new Vector2(0.5f, 1);

            var expanded = false;
            if (currentXmlElement.HasAttribute("expanded"))
            {
                expanded = currentXmlElement.attributes["expanded"].ToBoolean();
            }

            if (currentXmlElement.HasAttribute("contentHeight"))
            {
                section.contentHeight = currentXmlElement.attributes["contentHeight"].ToFloat();
            }

            section.expanded = expanded;
            section.Init();
        }
    }
}