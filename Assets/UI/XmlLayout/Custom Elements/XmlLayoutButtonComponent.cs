using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public class XmlLayoutButtonComponent : Button
    {
        private XmlLayoutButton m_xmlLayoutButton;
        protected XmlLayoutButton xmlLayoutButton
        {
            get
            {
                if (m_xmlLayoutButton == null) m_xmlLayoutButton = this.GetComponent<XmlLayoutButton>();

                return m_xmlLayoutButton;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);            

            xmlLayoutButton.NotifyButtonStateChanged((XmlElement.SelectionState)state);
        }
    }
}
