#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public class XmlLayoutList : MonoBehaviour
    {
        public XmlElement itemTemplate;
        public string DataSource;

        private XmlElement _listElement = null;
        public XmlElement listElement
        {
            get
            {
                if (_listElement == null) _listElement = this.GetComponent<XmlElement>();

                return _listElement;
            }
        }

        private RectTransform _rectTransform = null;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = this.transform as RectTransform;

                return _rectTransform;
            }
        }

        public List<XmlLayoutListItem> listItems = new List<XmlLayoutListItem>();

        public int baseSiblingIndex { get; set; }

        public IObservableList list { get; set; }

        public bool isCalculatedList { get; set; }

        public string itemShowAnimation = "None";
        public string itemHideAnimation = "None";
        public float itemAnimationDuration = 0.25f;
    }
}
#endif
