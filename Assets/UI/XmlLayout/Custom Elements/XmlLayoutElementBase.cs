using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI.Xml
{
    public abstract class XmlLayoutElementBase : MonoBehaviour
    {
        private XmlElement _xmlElement = null;
        /// <summary>
        /// The 'XmlElement' of this GameObject
        /// </summary>
        public XmlElement xmlElement
        {
            get
            {
                if (_xmlElement == null) _xmlElement = GetComponent<XmlElement>();

                return _xmlElement;
            }
        }

        /// <summary>
        /// The Unity 'RectTransform' attached to this XmlElement
        /// </summary>
        public RectTransform rectTransform
        {
            get
            {
                return xmlElement.rectTransform;
            }
        }

        /// <summary>
        /// The Unity 'Layout Element' used by this XmlElement
        /// </summary>
        public LayoutElement layoutElement
        {
            get
            {
                return xmlElement.layoutElement;
            }
        }
    }
}
