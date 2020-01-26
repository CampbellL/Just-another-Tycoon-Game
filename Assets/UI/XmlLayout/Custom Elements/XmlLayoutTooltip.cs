using System;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;
using System.Linq;

namespace UI.Xml
{
    public class XmlLayoutTooltip : MonoBehaviour
    {
        public Text TextComponent;
        public Outline OutlineComponent;
        public Image BackgroundComponent;
        public Image BorderComponent;

        protected RectTransform m_rectTransform;
        protected RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null) m_rectTransform = this.transform as RectTransform;

                return m_rectTransform;
            }
        }

        protected Canvas m_canvas;
        protected Canvas canvas
        {
            get
            {
                if (m_canvas == null) m_canvas = GetComponentInParent<Canvas>();

                return m_canvas;
            }
        }

        public TooltipPosition tooltipPosition = TooltipPosition.Right;
        public float offsetDistance = 8f;
        public bool followMouse = false;

        public float fadeTime = 0.2f;
        public float showDelayTime = 0.1f;

        public float width = 0f;

        public TextComponentWrapper TextComponentWrapper;

        private bool started = false;

        private ContentSizeFitter contentSizeFitter;

        internal XmlElement currentElement = null;

        void Update()
        {
            // Always on top
            this.transform.SetAsLastSibling();

            if (followMouse)
            {
                SetPositionAdjacentToCursor();
            }

            if (currentElement != null && !currentElement.gameObject.activeInHierarchy)
            {
                FadeOut();
            }
        }

        private void Start()
        {
            if (started) return;

            TextComponentWrapper = new TextComponentWrapper(TextComponent);
            TextComponentWrapper.xmlElement.Initialise(xmlLayout, TextComponentWrapper.xmlElement.rectTransform, XmlLayoutUtilities.GetXmlTagHandler("Text"));

            contentSizeFitter = this.GetComponent<ContentSizeFitter>();

            started = true;
        }

        public void SetText(string text)
        {
            //TextComponent.text = text;
            TextComponentWrapper.text = text;
        }

        public void SetTextColor(Color color)
        {
            //TextComponent.color = color;
            TextComponentWrapper.color = color;
        }

        public void SetBackgroundColor(Color color)
        {
            BackgroundComponent.color = color;
        }

        public void SetBackgroundImage(Sprite image)
        {
            BackgroundComponent.sprite = image;
        }

        public void SetBorderColor(Color color)
        {
            BorderComponent.color = color;
        }

        public void SetBorderImage(Sprite image)
        {
            BorderComponent.sprite = image;
        }

        public void SetFontSize(int size)
        {
            //TextComponent.fontSize = size;
            TextComponentWrapper.xmlElement.SetAndApplyAttribute("fontSize", size.ToString());
        }

        [Obsolete("Please use SetFont(string) instead")]
        public void SetFont(Font font)
        {
            TextComponent.font = font;
        }

        public void SetFont(string font)
        {
            TextComponentWrapper.xmlElement.SetAndApplyAttribute("font", font);
        }

        public void SetTooltipPadding(RectOffset padding)
        {
            BackgroundComponent.GetComponent<HorizontalOrVerticalLayoutGroup>().padding = padding;
        }

        public void SetTextOutlineColor(Color color)
        {
            if (color == default(Color))
            {
                OutlineComponent.enabled = false;
            }
            else
            {
                OutlineComponent.enabled = true;
                OutlineComponent.effectColor = color;
            }
        }

        public void SetStylesFromXmlElement(XmlElement element)
        {
            LoadAttributes(element.attributes);
        }

        public void SetPositionAdjacentTo(XmlElement element)
        {
            if (element == null) return;

            var xmlLayoutTransform = xmlLayout.transform as RectTransform;

            rectTransform.pivot = GetPivotForPosition(tooltipPosition);

            var pivotOffset = new Vector2(1 - rectTransform.pivot.x, 1 - rectTransform.pivot.y);

            var elementDimensions = element.rectTransform.rect.size;

            var elementPositionRelativeToXmlLayout = xmlLayout.transform.InverseTransformPoint(element.rectTransform.position);


            // this accounts for any variance in the XmlLayout's pivot if it is off-center
            elementPositionRelativeToXmlLayout -= new Vector3(xmlLayoutTransform.rect.width * (0.5f - xmlLayoutTransform.pivot.x),
                                                              xmlLayoutTransform.rect.height * (0.5f - xmlLayoutTransform.pivot.y), 0);

            rectTransform.anchoredPosition3D =
                new Vector3(elementPositionRelativeToXmlLayout.x + ((pivotOffset.x - element.rectTransform.pivot.x) * elementDimensions.x),
                            elementPositionRelativeToXmlLayout.y + ((pivotOffset.y - element.rectTransform.pivot.y) * elementDimensions.y),
                            0);

            // Apply any offset
            switch (tooltipPosition)
            {
                case TooltipPosition.Right:
                {
                    rectTransform.anchoredPosition3D += new Vector3(offsetDistance, 0, 0);
                }
                break;

                case TooltipPosition.Left:
                {
                    rectTransform.anchoredPosition3D += new Vector3(-offsetDistance, 0, 0);
                }
                break;

                case TooltipPosition.Above:
                {
                    rectTransform.anchoredPosition3D += new Vector3(0, offsetDistance, 0);
                }
                break;

                case TooltipPosition.Below:
                {
                    rectTransform.anchoredPosition3D += new Vector3(0, -offsetDistance, 0);
                }
                break;
            }

            rectTransform.localRotation = new Quaternion(0, 0, 0, 0);
            ClampWithinCanvas();
        }

        public void SetPositionAdjacentToCursor()
        {
            rectTransform.pivot = GetPivotForPosition(tooltipPosition);

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                Vector2 position = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, (Vector2)Input.mousePosition, Camera.main, out position);

                rectTransform.position = canvas.transform.TransformPoint(position);
            }
            else
            {
                rectTransform.position = Input.mousePosition;
            }

            rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0);

            var offset = new Vector2();

            switch (tooltipPosition)
            {
                case TooltipPosition.Above:
                    offset.y = offsetDistance;
                    break;
                case TooltipPosition.Below:
                    offset.y = -offsetDistance - rectTransform.rect.height;
                    break;
                case TooltipPosition.Left:
                    offset.x = -offsetDistance;
                    break;
                case TooltipPosition.Right:
                    offset.x = offsetDistance;
                    break;
            }

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                var canvasScale = (Vector2)canvas.transform.localScale;
                offset = new Vector2(canvasScale.x * offset.x, canvasScale.y * offset.y);
            }

            rectTransform.position = rectTransform.position + (Vector3)offset;

            ClampWithinCanvas();
        }

        protected void ClampWithinCanvas()
        {
            var canvasRect = (canvas.transform as RectTransform).rect;

            var minPosition = canvasRect.min - rectTransform.rect.min;
            var maxPosition = canvasRect.max - rectTransform.rect.max;

            var clampedPosition = new Vector3();

            clampedPosition.x = Mathf.Clamp(rectTransform.anchoredPosition.x, minPosition.x, maxPosition.x);
            clampedPosition.y = Mathf.Clamp(rectTransform.anchoredPosition.y, minPosition.y, maxPosition.y);

            rectTransform.anchoredPosition = clampedPosition;
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0);
        }

        public enum TooltipPosition
        {
            Above,
            Below,
            Left,
            Right
        }

        protected Vector2 GetPivotForPosition(TooltipPosition position)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            switch (position)
            {
                case TooltipPosition.Above:
                    pivot = new Vector2(0.5f, 0);
                    break;
                case TooltipPosition.Below:
                    pivot = new Vector2(0.5f, 1);
                    break;
                case TooltipPosition.Left:
                    pivot = new Vector2(1, 0.5f);
                    break;
                case TooltipPosition.Right:
                    pivot = new Vector2(0, 0.5f);
                    break;
            }

            return pivot;
        }

        private XmlLayout _xmlLayout = null;
        private XmlLayout xmlLayout
        {
            get
            {
                if (_xmlLayout == null) _xmlLayout = this.GetComponentInParent<XmlLayout>();
                return _xmlLayout;
            }
        }

        private CanvasGroup _canvasGroup = null;
        private CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null) _canvasGroup = this.GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

#if TEXTMESHPRO_PRESENT
        public void ToggleTextMeshPro (bool on)
        {
            if (on)
            {
                // if we're already using TextMeshPro; do nothing
                if (TextComponentWrapper != null
                 && TextComponentWrapper.xmlElement != null
                 && TextComponentWrapper.xmlElement.tagType == "TextMeshPro") return;

                var tmp = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                XmlElement tmpXmlElement = null;

                if(tmp == null)
                {
                    // create instance
                    var tagHandler = XmlLayoutUtilities.GetXmlTagHandler("TextMeshPro");
                    tmpXmlElement = tagHandler.GetInstance(this.rectTransform, xmlLayout);
                    tagHandler.SetInstance(tmpXmlElement);
                    tagHandler.ApplyAttributes(new AttributeDictionary());

                    tmp = tmpXmlElement.GetComponent<TMPro.TextMeshProUGUI>();

                    tmp.rectTransform.localScale = Vector3.one;
                }

                tmpXmlElement = tmp.GetComponent<XmlElement>();

                TextComponentWrapper = new TextComponentWrapper(tmp);

                // hide the regular text component
                TextComponent.gameObject.SetActive(false);

                // enable the TMP object if it wasn't already
                tmp.gameObject.SetActive(true);
            }
            else
            {
                if (TextComponentWrapper != null
                && TextComponentWrapper.xmlElement != null
                && TextComponentWrapper.xmlElement.tagType == "Text") return;

                TextComponentWrapper = new TextComponentWrapper(TextComponent);

                TextComponent.gameObject.SetActive(true);

                var tmp = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null) tmp.gameObject.SetActive(false);
            }
        }
#endif

        public void LoadAttributes(AttributeDictionary attributes)
        {
            if (!started) Start();

#if TEXTMESHPRO_PRESENT
            if (attributes.ContainsKey("tooltipUseTextMeshPro")) ToggleTextMeshPro(attributes["tooltipUseTextMeshPro"].ToBoolean());
#endif

            if (attributes.ContainsKey("tooltipTextColor")) SetTextColor(attributes["tooltipTextColor"].ToColor(xmlLayout));
            if (attributes.ContainsKey("tooltipBackgroundColor")) SetBackgroundColor(attributes["tooltipBackgroundColor"].ToColor(xmlLayout));
            if (attributes.ContainsKey("tooltipBorderColor")) SetBorderColor(attributes["tooltipBorderColor"].ToColor(xmlLayout));

            if (attributes.ContainsKey("tooltipBackgroundImage")) SetBackgroundImage(attributes["tooltipBackgroundImage"].ToSprite());
            if (attributes.ContainsKey("tooltipBorderImage")) SetBorderImage(attributes["tooltipBorderImage"].ToSprite());

            if (attributes.ContainsKey("tooltipFontSize")) SetFontSize(int.Parse(attributes["tooltipfontsize"]));
            if (attributes.ContainsKey("tooltipPadding")) SetTooltipPadding(attributes["tooltipPadding"].ToRectOffset());

            if (attributes.ContainsKey("tooltipTextOutlineColor")) SetTextOutlineColor(attributes["tooltipTextOutlineColor"].ToColor(xmlLayout));
            if (attributes.ContainsKey("tooltipFont")) SetFont(attributes["tooltipFont"]);

            if (attributes.ContainsKey("tooltipPosition")) tooltipPosition = (TooltipPosition)Enum.Parse(typeof(TooltipPosition), attributes["tooltipPosition"]);
            if (attributes.ContainsKey("tooltipFollowMouse")) followMouse = attributes["tooltipFollowMouse"].ToBoolean();
            if (attributes.ContainsKey("tooltipOffset")) offsetDistance = float.Parse(attributes["tooltipOffset"]);

            if (attributes.ContainsKey("tooltipFadeTime")) fadeTime = attributes["tooltipFadeTime"].ToFloat();
            if (attributes.ContainsKey("tooltipDelayTime")) showDelayTime = attributes["tooltipDelayTime"].ToFloat();

            if (attributes.ContainsKey("tooltipWidth"))
            {
                width = attributes["tooltipWidth"].ToFloat();
            }
            else
            {
                width = 0;
            }
        }

        public void FadeIn()
        {
            StopAllCoroutines();

            if (width == 0)
            {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            gameObject.SetActive(true);

            if (fadeTime > 0)
            {
                canvasGroup.alpha = 0;
                StartCoroutine(FadeInCoroutine(fadeTime));
            }
            else
            {
                canvasGroup.alpha = 1;
            }
        }

        private System.Collections.IEnumerator FadeInCoroutine(float fadeInTime)
        {
            float startTime = Time.unscaledTime;
            float endTime = startTime + fadeInTime;

            while(Time.unscaledTime <= endTime)
            {
                float percentage = (Time.unscaledTime - startTime) / fadeInTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, percentage);

                yield return null;
            }
        }

        public void FadeOut()
        {
            canvasGroup.alpha = 1;

            if (fadeTime > 0)
            {
                if(gameObject.activeInHierarchy) StartCoroutine(FadeOutCoroutine(fadeTime));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator FadeOutCoroutine(float fadeOutTime)
        {
            float startTime = Time.unscaledTime;
            float endTime = startTime + fadeOutTime;

            while (Time.unscaledTime <= endTime)
            {
                float percentage = (Time.unscaledTime - startTime) / fadeOutTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, percentage);

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
