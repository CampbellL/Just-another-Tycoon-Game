using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UI.Xml
{
    public class XmlLayoutScrollRect : ScrollRect
    {
        private XmlElement _xmlElement = null;
        private XmlElement xmlElement
        {
            get
            {
                if (_xmlElement == null) _xmlElement = GetComponent<XmlElement>();
                return _xmlElement;
            }
        }

        private List<XmlElement> _parentXmlElements = null;
        private List<XmlElement> parentXmlElements
        {
            get
            {
                if (_parentXmlElements == null) _parentXmlElements = GetComponentsInParent<XmlElement>().ToList();
                return _parentXmlElements;
            }
        }

#if UNITY_2018_1_OR_NEWER
        protected override void SetNormalizedPosition(float value, int axis)
        {
            // if this element, or any of its parents are animating, ignore calls to change its normalized position
            if (xmlElement.IsAnimating) return;
            if (parentXmlElements.Any(e => e.IsAnimating)) return;

            base.SetNormalizedPosition(value, axis);
        }
#endif

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            // if this element, or any of its parents are animating, ignore calls to change its anchored position
            if (xmlElement.IsAnimating) return;
            if (parentXmlElements.Any(e => e.IsAnimating)) return;

            base.SetContentAnchoredPosition(position);
        }

        protected override void OnBeforeTransformParentChanged()
        {
            // reset the parent XmlElement collection; it will be repopulated the next time it is called
            _parentXmlElements = null;
        }
    }
}
