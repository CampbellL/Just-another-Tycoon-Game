using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{        
    public class XmlLayoutListItem : MonoBehaviour
    {
        public string guid;

        private XmlElement _xmlElement;
        public XmlElement xmlElement
        {
            get
            {
                if (_xmlElement == null) _xmlElement = this.GetComponent<XmlElement>();

                return _xmlElement;
            }
        }
    }
}
