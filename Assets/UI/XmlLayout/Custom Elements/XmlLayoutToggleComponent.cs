using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;

namespace UI.Xml
{    
    public class XmlLayoutToggleComponent : Toggle
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
    }
}
