using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UI.Xml
{
    public partial class XmlLayoutController : MonoBehaviour
    {
        private List<IXmlElementReference> xmlElementReferences = new List<IXmlElementReference>();

        /// <summary>
        /// Create a reference to the specified XmlElement by id which will persist through layout rebuilds.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XmlElementReference<XmlElement> XmlElementReference(string id)
        {
            return XmlElementReference<XmlElement>(id);
        }

        /// <summary>
        /// Create a reference to a specific MonoBehaviour on an XmlElement object (by id) which will persist through layout rebuilds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public XmlElementReference<T> XmlElementReference<T>(string id)
            where T : MonoBehaviour
        {
            var reference = new XmlElementReference<T>(xmlLayout, id);

            xmlElementReferences.Add(reference);

            return reference;
        }

        internal void NotifyXmlElementReferencesOfLayoutRebuild()
        {
            // If we don't do this, any calls to these elements in the same frame the layout was rebuilt may still refer to the 'old' elements
            xmlElementReferences.ForEach(x => x.ClearElement());
        }
    }
}
