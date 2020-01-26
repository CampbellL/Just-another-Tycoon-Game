using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public class XmlLayoutDragEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool IsBeingDragged = false;

        Vector2 OriginalPivotOnDragStart = Vector2.zero;
        Vector2 OriginalPositionOnDragStart = Vector2.zero;

        int OriginalSiblingIndex = 0;

        private RectTransform rectTransform = null;
        private XmlElement xmlElement = null;

        private Canvas parentCanvas = null;
        private RectTransform parentCanvasTransform = null;


        void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
            xmlElement = this.GetComponent<XmlElement>();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!xmlElement.AllowDragging || eventData == null) return;

            // this is the element being dragged
            XmlElement.ElementCurrentlyBeingDragged = xmlElement;

            if (!IsBeingDragged)
            {
                //  Begin Drag
                OriginalPivotOnDragStart = rectTransform.pivot;
                OriginalPositionOnDragStart = rectTransform.anchoredPosition3D;
                OriginalSiblingIndex = rectTransform.GetSiblingIndex();

                rectTransform.SetAsLastSibling();

                if (!xmlElement.RestrictDraggingToParentBounds) rectTransform.SetParent(xmlElement.xmlLayoutInstance.XmlElement.rectTransform);

                xmlElement.CanvasGroup.blocksRaycasts = false;
            }

            // Old approach (using event delta):
            //rectTransform.anchoredPosition += eventData.delta;

            // New approach (using event position):
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = eventData.position;
            }
            else
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasTransform, eventData.position, parentCanvas.worldCamera ?? Camera.main, out position);
                rectTransform.position = parentCanvasTransform.TransformPoint(position);
            }

            // Clamp within parent bounds
            if (xmlElement.RestrictDraggingToParentBounds)
            {
                var parentTransform = xmlElement.parentElement.rectTransform;

                Vector3 pos = parentTransform.localPosition;

                Vector3 minPosition = parentTransform.rect.min - rectTransform.rect.min;
                Vector3 maxPosition = parentTransform.rect.max - rectTransform.rect.max;

                pos.x = Mathf.Clamp(rectTransform.localPosition.x, minPosition.x, maxPosition.x);
                pos.y = Mathf.Clamp(rectTransform.localPosition.y, minPosition.y, maxPosition.y);

                rectTransform.localPosition = pos;
            }

            this.IsBeingDragged = true;

            xmlElement.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsBeingDragged) return;
            if (xmlElement == XmlElement.ElementCurrentlyBeingDragged) XmlElement.ElementCurrentlyBeingDragged = null;

            IsBeingDragged = false;
            rectTransform.SetParent(xmlElement.parentElement.rectTransform);
            rectTransform.SetSiblingIndex(OriginalSiblingIndex);

            if (xmlElement.ReturnToOriginalPositionWhenReleased)
            {
                rectTransform.pivot = OriginalPivotOnDragStart;
                rectTransform.anchoredPosition3D = OriginalPositionOnDragStart;
            }

            // Resume blocking raycasts
            xmlElement.CanvasGroup.blocksRaycasts = true;

            xmlElement.OnEndDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentCanvas = xmlElement.GetComponentInParent<Canvas>();
            parentCanvasTransform = parentCanvas.transform as RectTransform;

            xmlElement.OnBeginDrag(eventData);
        }
    }
}
