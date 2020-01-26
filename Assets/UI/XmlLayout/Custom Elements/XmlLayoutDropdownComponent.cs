using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;

namespace UI.Xml
{    
    public class XmlLayoutDropdownComponent : Dropdown
    {
        private XmlElement m_xmlElement = null;
        private XmlElement xmlElement
        {
            get
            {
                if (m_xmlElement == null) m_xmlElement = this.GetComponent<XmlElement>();
                return m_xmlElement;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);            
            
            if (xmlElement != null) xmlElement.NotifySelectionStateChanged((XmlElement.SelectionState)state);            
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            var item = base.CreateItem(itemTemplate);
            var rectTransform = item.GetComponent <RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;            

            return item;
        }
    }
}
