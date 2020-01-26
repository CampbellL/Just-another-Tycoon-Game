using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public class XmlLayoutAccordionSection : XmlLayoutElementBase
    {
        public XmlLayoutAccordionSectionHeader header;
        public XmlLayoutAccordionSectionContent content;

        public bool expanded = false;
        public float contentHeight = 128.0f;

        private XmlLayoutAccordionSectionHeaderIcon _icon;
        public XmlLayoutAccordionSectionHeaderIcon headerIcon
        {
            get
            {
                if (_icon == null)
                {
                    if (header == null) return null;

                    _icon = header.GetComponentInChildren<XmlLayoutAccordionSectionHeaderIcon>();
                }

                return _icon;
            }
        }

        private XmlLayoutAccordion _accordion = null;
        public XmlLayoutAccordion accordion
        {
            get
            {
                if (_accordion == null) _accordion = GetComponentInParent<XmlLayoutAccordion>();

                return _accordion;
            }
        }

        private bool wasExpandedBeforeDrag = false;

        internal void Init()
        {
            if (headerIcon != null) headerIcon.Init();

            if (expanded)
            {
                SetExpanded();
                content.layoutElement.preferredHeight = contentHeight;
            }
            else
            {
                SetCollapsed();
                content.layoutElement.preferredHeight = 0;
            }

            SetLayoutHeight();

            xmlElement.AddOnBeginDragEvent(DragStart);
            xmlElement.AddOnEndDragEvent(DragEnd);
        }

        internal void SetExpanded()
        {
            expanded = true;

            xmlElement.RemoveClass("collapsed");
            xmlElement.AddClass("expanded");

            if (headerIcon != null)
            {
                headerIcon.SetExpanded();
            }

            if (content != null) content.xmlElement.CanvasGroup.interactable = true;
        }

        public void Expand()
        {
            if (expanded) return;

            if (content != null)
            {
                SetExpanded();

                StartCoroutine(AnimateHeight(GetDesiredLayoutHeight(), contentHeight));
            }
        }

        internal void SetCollapsed()
        {
            expanded = false;

            xmlElement.RemoveClass("expanded");
            xmlElement.AddClass("collapsed");

            if (headerIcon != null)
            {
                headerIcon.SetCollapsed();
            }

            if (content != null) content.xmlElement.CanvasGroup.interactable = false;
        }

        public void Collapse()
        {
            if (!expanded) return;

            if (content != null)
            {
                SetCollapsed();

                StartCoroutine(AnimateHeight(GetDesiredLayoutHeight(), 0));
            }
        }

        internal float GetDesiredLayoutHeight()
        {
            if (expanded)
            {
                return GetHeaderHeight() + contentHeight;
            }
            else
            {
                return GetHeaderHeight();
            }
        }

        internal void SetLayoutHeight()
        {
            layoutElement.preferredHeight = GetDesiredLayoutHeight();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutElement.preferredHeight);
        }

        internal float GetHeaderHeight()
        {
            return header != null ? header.layoutElement.preferredHeight : 0;
        }

        protected virtual IEnumerator AnimateHeight(float sectionHeight, float contentHeight)
        {
            if (accordion.accordionAnimationDuration > 0)
            {
                float rate = 1.0f / accordion.accordionAnimationDuration;
                float index = 0f;
                float initialSectionHeight = layoutElement.preferredHeight;
                float initialContentHeight = content.layoutElement.preferredHeight;

                while (index < 1)
                {
                    layoutElement.preferredHeight = Mathf.Lerp(initialSectionHeight, sectionHeight, index);
                    content.layoutElement.preferredHeight = Mathf.Lerp(initialContentHeight, contentHeight, index);

                    index += rate * (xmlElement.xmlLayoutInstance.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

                    yield return null;
                }
            }

            layoutElement.preferredHeight = sectionHeight;
            content.layoutElement.preferredHeight = contentHeight;
        }

        internal void DragStart()
        {
            wasExpandedBeforeDrag = expanded;

            SetCollapsed();
            content.layoutElement.preferredHeight = 0;
            SetLayoutHeight();

            if (accordion.makeDraggedSectionTransparent) xmlElement.CanvasGroup.alpha = 0.5f;

            accordion.SectionDragStart(this);
        }

        internal void DragEnd()
        {
            if (wasExpandedBeforeDrag)
            {
                Expand();
            }

            accordion.SectionDragEnd();

            if (accordion.makeDraggedSectionTransparent) xmlElement.CanvasGroup.alpha = 1f;
        }
    }
}
