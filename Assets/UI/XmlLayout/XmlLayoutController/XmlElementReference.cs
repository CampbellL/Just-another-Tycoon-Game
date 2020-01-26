using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml
{
    internal interface IXmlElementReference
    {
        void ClearElement();
    }

    /// <summary>
    /// Creates a reference to an XmlElement (using the id attribute) which will persist through layout rebuilds automatically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlElementReference<T> : IXmlElementReference
        where T : MonoBehaviour
    {
        private T _element;
        public T element
        {
            get
            {                
                if (_element == null)
                {
                    if (useInternalId)
                    {
                        _element = xmlLayout.XmlElement.GetElementByInternalId<T>(id);
                    }
                    else
                    {
                        _element = xmlLayout.GetElementById<T>(id);
                    }
                }
                return _element;
            }

            protected set
            {
                _element = value;
            }
        }

        private XmlLayout xmlLayout;
        private string id;
        private bool useInternalId = false;

        public XmlElementReference(XmlLayout xmlLayout, string id, bool useInternalId = false)
        {            
            this.xmlLayout = xmlLayout;
            this.id = id;
            this.useInternalId = useInternalId;
        }

        static public implicit operator T(XmlElementReference<T> getXmlElement)
        {
            return getXmlElement.element;
        }

        public void ClearElement()
        {
            _element = null;
        }
    }
}
