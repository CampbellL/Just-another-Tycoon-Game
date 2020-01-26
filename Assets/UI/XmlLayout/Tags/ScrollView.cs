using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

namespace UI.Xml.Tags
{
    public abstract class ScrollViewTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<ScrollRect>();
            }
        }

        /*private XmlLayoutScrollView _xmlLayoutScrollView;
        public XmlLayoutScrollView xmlLayoutScrollView
        {
            get
            {
                if (_xmlLayoutScrollView == null)
                {
                    _xmlLayoutScrollView = primaryComponent.gameObject.AddComponent<XmlLayoutScrollView>();
                    _xmlLayoutScrollView.scrollRect = primaryComponent as ScrollRect;
                }

                return _xmlLayoutScrollView;
            }
        }*/

        public override RectTransform transformToAddChildrenTo
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                var scrollRect = (ScrollRect)primaryComponent;

                return scrollRect.content;
            }
        }

        private List<string> _eventAttributeNames = new List<string>()
        {
            "onClick",
            "onMouseEnter",
            "onMouseExit",
            "onValueChanged",
            "onMouseUp",
            "onMouseDown"
        };

        protected override List<string> eventAttributeNames
        {
            get
            {
                return _eventAttributeNames;
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            base.ApplyAttributes(attributesToApply);
            var scrollView = (ScrollRect)primaryComponent;

            if (attributesToApply.ContainsKey("noscrollbars") && attributesToApply["noscrollbars"].ToBoolean())
            {
                if (scrollView.verticalScrollbar != null)
                {
                    Destroy(scrollView.verticalScrollbar.gameObject);
                    scrollView.verticalScrollbar = null;
                }

                if (scrollView.horizontalScrollbar != null)
                {
                    Destroy(scrollView.horizontalScrollbar.gameObject);
                    scrollView.horizontalScrollbar = null;
                }
            }

            scrollView.viewport.offsetMax = new Vector2();
            scrollView.viewport.offsetMin = new Vector2();

            var hasVerticalScrollbar = scrollView.verticalScrollbar != null;
            var hasHorizontalScrollbar = scrollView.horizontalScrollbar != null;

            if (attributesToApply.ContainsKey("scrollbarbackgroundcolor"))
            {
                var color = attributesToApply["scrollbarbackgroundcolor"].ToColor(currentXmlLayoutInstance);

                if (hasVerticalScrollbar) scrollView.verticalScrollbar.GetComponent<Image>().color = color;
                if (hasHorizontalScrollbar) scrollView.horizontalScrollbar.GetComponent<Image>().color = color;
            }

            if (attributesToApply.ContainsKey("scrollbarbackgroundimage"))
            {
                var image = attributesToApply["scrollbarbackgroundimage"].ToSprite();

                if (hasVerticalScrollbar) scrollView.verticalScrollbar.GetComponent<Image>().sprite = image;
                if (hasHorizontalScrollbar) scrollView.horizontalScrollbar.GetComponent<Image>().sprite = image;
            }

            if (attributesToApply.ContainsKey("scrollbarcolors"))
            {
                var colors = attributesToApply["scrollbarcolors"].ToColorBlock(currentXmlLayoutInstance);

                if (hasVerticalScrollbar) scrollView.verticalScrollbar.colors = colors;
                if (hasHorizontalScrollbar) scrollView.horizontalScrollbar.colors = colors;
            }

            if (attributesToApply.ContainsKey("scrollbarimage"))
            {
                var image = attributesToApply["scrollbarimage"].ToSprite();
                if (hasVerticalScrollbar) scrollView.verticalScrollbar.image.sprite = image;
                if (hasHorizontalScrollbar) scrollView.horizontalScrollbar.image.sprite = image;
            }

            if (hasVerticalScrollbar)
            {
                if (attributesToApply.ContainsKey("verticalscrollbarwidth"))
                {
                    var scrollbarWidth = float.Parse(attributesToApply["verticalscrollbarwidth"]);
                    (scrollView.viewport.transform as RectTransform).offsetMax = new Vector2(-scrollbarWidth, 0);
                    (scrollView.verticalScrollbar.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollbarWidth);
                }
                else
                {
                    (scrollView.viewport.transform as RectTransform).offsetMax = new Vector2(-17, 0);
                }
            }

            if (hasHorizontalScrollbar)
            {
                if (attributesToApply.ContainsKey("horizontalscrollbarheight"))
                {
                    var scrollbarheight = float.Parse(attributesToApply["horizontalscrollbarheight"]);
                    (scrollView.viewport.transform as RectTransform).offsetMin = new Vector2(0, scrollbarheight);
                    (scrollView.horizontalScrollbar.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollbarheight);
                }
                else
                {
                    (scrollView.viewport.transform as RectTransform).offsetMin = new Vector2(0, 17);
                }
            }

            if (attributesToApply.ContainsKey("maskimage"))
            {
                var viewportImage = scrollView.viewport.GetComponent<Image>();
                var maskImageName = attributesToApply["maskimage"];
                if (!string.IsNullOrEmpty(maskImageName))
                {
                    viewportImage.sprite = maskImageName.ToSprite();
                }
                else
                {
                    viewportImage.sprite = null;
                }
            }

            /*
            // ScrollRect just overrides this value
            if (attributes.ContainsKey("scrollbarsize"))
            {
                var size = float.Parse(attributes["scrollbarsize"]);
                if (hasVerticalScrollbar) scrollView.verticalScrollbar.size = size;
                if (hasHorizontalScrollbar) scrollView.horizontalScrollbar.size = size;
            }*/
        }

        void Destroy(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(o);
            }
            else
            {
                GameObject.DestroyImmediate(o);
            }
        }

        public override void Close()
        {
            base.Close();

            var scrollRect = ((ScrollRect)primaryComponent);
            var content = scrollRect.content;

            XmlLayoutTimer.DelayedCall(0.05f, () =>
            {
                var simpleContentSizeFitter = content.GetComponent<SimpleContentSizeFitter>();
                simpleContentSizeFitter.MatchChildDimensions();

            }, scrollRect);
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var scrollRect = (ScrollRect)primaryComponent;
                        var transform = currentInstanceTransform;

                        var eventData = GetEventValueData(eventValue);

                        scrollRect.onValueChanged.AddListener((e) =>
                        {
                            string _value = eventData.value;
                            var valueLower = eventData.value.ToLower();

                            if (valueLower == "selectedvalue" || valueLower == "xy")
                            {
                                _value = string.Format("{0},{1}", e.x, e.y);
                            }
                            else if (valueLower == "x")
                            {
                                _value = e.x.ToString();
                            }
                            else if (valueLower == "y")
                            {
                                _value = e.y.ToString();
                            }

                            currentXmlLayoutInstance.XmlLayoutController.ReceiveMessage(eventData.methodName, _value, transform, eventData.parameters);
                        });
                    }
                    break;

                default:
                    base.HandleEventAttribute(eventName, eventValue);
                    break;
            }
        }
    }

    public class VerticalScrollViewTagHandler : ScrollViewTagHandler { };
    public class HorizontalScrollViewTagHandler : ScrollViewTagHandler { };
}
