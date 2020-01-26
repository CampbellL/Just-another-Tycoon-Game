using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI.Xml
{    
    public class XmlLayoutAccordionSectionContent : XmlLayoutElementBase
    {
        private XmlLayoutAccordionSection _section = null;
        internal XmlLayoutAccordionSection section
        {
            get
            {
                if (_section == null) _section = GetComponentInParent<XmlLayoutAccordionSection>();

                return _section;
            }
        }
    }
}
