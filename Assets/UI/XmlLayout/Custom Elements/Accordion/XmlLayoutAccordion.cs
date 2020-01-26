using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public class XmlLayoutAccordion : XmlLayoutElementBase
    {
        public bool collapsible = false;
        public float accordionAnimationDuration = 0.25f;
        public List<XmlLayoutAccordionSection> sections = new List<XmlLayoutAccordionSection>();
        public bool allowSectionReordering = false;
        public bool expandSectionAfterReorder = true;
        public bool makeDraggedSectionTransparent = true;

        private bool dragInProgress = false;
        internal XmlLayoutAccordionSection sectionBeingDragged = null;
        private Coroutine placeHolderAnimationCoroutine = null;
        private bool placeHolderAnimationInProgress = false;

        private XmlLayoutAccordionSectionPlaceholder _sectionPlaceholder = null;
        internal XmlLayoutAccordionSectionPlaceholder sectionPlaceholder
        {
            get
            {
                if (_sectionPlaceholder == null)
                {
                    var sectionPlaceHolderGO = GameObject.Instantiate(XmlLayoutUtilities.LoadResource<GameObject>("XmlLayout Prefabs/Accordion/AccordionSectionPlaceholder"));
                    _sectionPlaceholder = sectionPlaceHolderGO.GetComponent<XmlLayoutAccordionSectionPlaceholder>();
                    _sectionPlaceholder.xmlElement.rectTransform.SetParent(xmlElement.rectTransform);
                }

                return _sectionPlaceholder;
            }
        }

        private Canvas _canvas = null;
        private Canvas canvas
        {
            get
            {
                if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

                return _canvas;
            }
        }

        private RectTransform _canvasTransform = null;
        private RectTransform canvasTransform
        {
            get
            {
                if (_canvasTransform == null) _canvasTransform = canvas.GetComponent<RectTransform>();

                return _canvasTransform;
            }
        }

        internal void SectionHeaderClicked(XmlLayoutAccordionSection section)
        {
            if (!section.expanded)
            {
                ExpandSection(section);
            }
            else
            {
                if (collapsible) CollapseSection(section);
            }
        }

        public void ExpandSection(XmlLayoutAccordionSection section)
        {
            if (!section.expanded)
            {
                foreach (var sectionToHide in sections)
                {
                    if (section != sectionToHide) sectionToHide.Collapse();
                }

                section.Expand();
            }
        }

        public void CollapseSection(XmlLayoutAccordionSection section)
        {
            section.Collapse();
        }

        internal void SectionDropped(XmlLayoutAccordionSection section)
        {
            if (!allowSectionReordering) return;

            var oldIndex = sections.IndexOf(section);
            sections.RemoveAt(oldIndex);

            int newIndex = sectionPlaceholder.rectTransform.GetSiblingIndex();
            if (newIndex > oldIndex) newIndex--;

            sections.Insert(newIndex, section);

            XmlLayoutTimer.AtEndOfFrame(() => section.transform.SetSiblingIndex(newIndex), this, true);

            foreach (var s in sections)
            {
                s.content.xmlElement.childElements.First().CanvasGroup.blocksRaycasts = true;
            }

            if (expandSectionAfterReorder)
            {
                ExpandSection(section);
            }
        }

        internal void SectionDragStart(XmlLayoutAccordionSection draggedSection)
        {
            dragInProgress = true;

            sectionBeingDragged = draggedSection;

            sectionPlaceholder.layoutElement.preferredHeight = 0;
            sectionPlaceholder.gameObject.SetActive(true);

            GrowPlaceholder();

            foreach(var section in sections)
            {
                section.content.xmlElement.childElements.First().CanvasGroup.blocksRaycasts = false;
            }

            sectionBeingDragged.xmlElement.layoutElement.ignoreLayout = true;
        }

        internal void SectionDragEnd()
        {
            dragInProgress = false;
            sectionBeingDragged.xmlElement.layoutElement.ignoreLayout = false;

            XmlLayoutTimer.AtEndOfFrame(() => sectionPlaceholder.gameObject.SetActive(false), this);

            SectionDropped(sectionBeingDragged);
        }

        internal void MovePlaceholderToPosition(int index)
        {
            if (placeHolderAnimationCoroutine != null) StopCoroutine(placeHolderAnimationCoroutine);

            placeHolderAnimationCoroutine = StartCoroutine(AnimatePlaceholder(index));
        }

        private IEnumerator AnimatePlaceholder(int index)
        {
            yield return StartCoroutine(ShrinkPlaceholder());

            sectionPlaceholder.rectTransform.SetSiblingIndex(index);

            yield return StartCoroutine(GrowPlaceholder());
        }

        private IEnumerator ShrinkPlaceholder()
        {
            yield return AnimatePlaceholderHeight(0, 0.1f);
        }

        private IEnumerator GrowPlaceholder()
        {
            yield return AnimatePlaceholderHeight(sectionBeingDragged.GetHeaderHeight(), 0.1f);
        }

        private IEnumerator AnimatePlaceholderHeight(float newHeight, float duration)
        {
            placeHolderAnimationInProgress = true;

            yield return null;

            float rate = 1.0f / duration;
            float index = 0f;
            float initialHeight = sectionPlaceholder.layoutElement.preferredHeight;

            while (index < 1)
            {
                sectionPlaceholder.layoutElement.preferredHeight = Mathf.Lerp(initialHeight, newHeight, index);

                index += rate * (xmlElement.xmlLayoutInstance.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

                yield return null;
            }

            sectionPlaceholder.layoutElement.preferredHeight = newHeight;
            placeHolderAnimationInProgress = false;
        }

        private float mouseY = 0;
        private int placeHolderPosition = int.MaxValue;

        private void Update()
        {
            if (!dragInProgress) return;
            if (sectionBeingDragged == null) return;
            if (placeHolderAnimationInProgress) return;

            sectionBeingDragged.xmlElement.rectTransform.SetAsLastSibling();

            // if the mouse hasn't moved, don't do anything
            if (Mathf.Round(mouseY) == Mathf.Round(Input.mousePosition.y)) return;
            mouseY = Input.mousePosition.y;

            Vector2 localPoint;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                localPoint = rectTransform.InverseTransformPoint(Input.mousePosition);
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera ?? Camera.main, out localPoint);
            }

            XmlLayoutAccordionSection closestSection = null;
            float closestDistance = float.MaxValue;
            float closestDistanceAbsolute = float.MaxValue;

            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i] == sectionBeingDragged) continue;

                var distance = localPoint.y - sections[i].rectTransform.localPosition.y;
                if (Mathf.Abs(distance) < closestDistanceAbsolute)
                {
                    closestDistance = distance;
                    closestDistanceAbsolute = Mathf.Abs(closestDistance);
                    closestSection = sections[i];
                }
            }

            if (closestSection != null)
            {
                var index = sections.IndexOf(closestSection);

                if (sections.IndexOf(sectionBeingDragged) < index)
                {
                    index--;
                }

                if (closestDistance < 0)
                {
                    index++;
                }

                if (placeHolderPosition != index)
                {
                    placeHolderPosition = index;

                    MovePlaceholderToPosition(index);
                }
            }
        }
    }
}
