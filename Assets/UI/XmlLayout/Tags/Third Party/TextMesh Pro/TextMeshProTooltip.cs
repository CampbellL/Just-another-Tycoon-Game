#if TEXTMESHPRO_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Xml;
using TMPro;
using UnityEngine.Events;
using System.Linq;

namespace UI.Xml.Tags
{    
    public class TooltipUseTextMeshProAttribute : CustomXmlAttribute
    {
        public override eAttributeGroup AttributeGroup
        {
            get
            {
                return eAttributeGroup.Tooltip;
            }
        }

        public override string ValueDataType
        {
            get
            {                
                return "xs:boolean";
            }
        }

        public override bool UsesApplyMethod { get { return false; } }
    }

}
#endif
