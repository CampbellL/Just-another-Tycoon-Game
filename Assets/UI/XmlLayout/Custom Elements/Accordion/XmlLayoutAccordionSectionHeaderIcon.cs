using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public class XmlLayoutAccordionSectionHeaderIcon : XmlLayoutElementBase
    {
        public Sprite collapsedSprite;
        public Color collapsedColor;

        public Sprite expandedSprite;
        public Color expandedColor;

        private Image _image;
        protected Image image
        {
            get
            {
                if (_image == null) _image = GetComponent<Image>();
                return _image;
            }
        }

        internal void Init()
        {
            SetCollapsed();
        }

        internal void SetExpanded()
        {
            if (expandedSprite != null) image.sprite = expandedSprite;
            if (expandedColor != default(Color)) image.color = expandedColor;
        }

        internal void SetCollapsed()
        {
            if (collapsedSprite != null) image.sprite = collapsedSprite;
            if (collapsedColor != default(Color)) image.color = collapsedColor;
        }
    }
}
