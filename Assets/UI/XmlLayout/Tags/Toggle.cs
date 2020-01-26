using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UI.Xml.Tags
{
    public partial class ToggleTagHandler : InputBaseTagHandler, IHasXmlFormValue
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Toggle>();
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

            var toggle = primaryComponent as Toggle;
            var checkMark = toggle.graphic as Image;
            var textComponent = currentXmlElement.GetComponentInChildren<Text>();

            if (textComponent != null && currentXmlElement.childElements.Count > 0)
            {
                var childAttributes = new AttributeDictionary();

                var child = currentXmlElement.childElements.First();
                if (child.tagType == "Text")
                {
                    if (textComponent.gameObject != child.gameObject)
                    {
                        // Replace the original text component with the child element
                        child.rectTransform.SetParent(textComponent.rectTransform.parent);

                        if (Application.isPlaying)
                            GameObject.Destroy(textComponent.gameObject);
                        else
                            GameObject.DestroyImmediate(textComponent.gameObject);


                        textComponent = child.GetComponent<Text>();

                        if (!child.HasAttribute("alignment"))
                        {
                            childAttributes.Add("alignment", "MiddleLeft");
                        }
                    }
                }
#if TEXTMESHPRO_PRESENT
                else if (child.tagType == "TextMeshPro")
                {
                    // Replace the original text component with the child element
                    child.rectTransform.SetParent(textComponent.rectTransform.parent);
                    if (Application.isPlaying)
                        GameObject.Destroy(textComponent.gameObject);
                    else
                        GameObject.DestroyImmediate(textComponent.gameObject);

                    if (!child.HasAttribute("alignment"))
                    {
                        childAttributes.Add("alignment", "Left");
                    }
                }
#endif

                if (!child.HasAttribute("text"))
                {
                    if (attributesToApply.ContainsKey("text"))
                    {
                        childAttributes.Add("text", attributesToApply["text"]);
                    }
                    else
                    {
                        // if the child has no text, and the parent has no text, disable the text component
                        if (!currentXmlElement.HasAttribute("text"))
                        {
                            childAttributes.Add("active", "false");
                        }
                    }
                }
                else
                {
                    // override the parent value if need be
                    attributesToApply.SetValue("text", child.GetAttribute("text"));
                }

                // default padding value as per standard toggle element
                if (!child.HasAttribute("padding")) childAttributes.Add("padding", "23 5 2 1");
                if (!child.HasAttribute("flexibleWidth")) childAttributes.Add("flexibleWidth", "1");

                if(childAttributes.Count > 0)
                {
                    child.ApplyAttributes(childAttributes);
                }
            }

            if (attributesToApply.ContainsKey("checkcolor"))
            {
                checkMark.color = attributesToApply["checkcolor"].ToColor(currentXmlLayoutInstance);
            }

            var targetGraphic = toggle.targetGraphic as Image;
            if (attributesToApply.ContainsKey("togglewidth"))
            {
                var width = float.Parse(attributesToApply["togglewidth"]);
                /*targetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

                if (textComponent != null)
                {
                    textComponent.rectTransform.localPosition = new Vector2(width, textComponent.rectTransform.localPosition.y);
                }*/

                var toggleLayoutElement = targetGraphic.GetComponent<LayoutElement>() ?? targetGraphic.gameObject.AddComponent<LayoutElement>();
                toggleLayoutElement.preferredWidth = width;
                toggleLayoutElement.minWidth = width;
            }

            if (attributesToApply.ContainsKey("toggleheight"))
            {
                var height = float.Parse(attributesToApply["toggleheight"]);
                /*targetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);*/

                var toggleLayoutElement = targetGraphic.GetComponent<LayoutElement>() ?? targetGraphic.gameObject.AddComponent<LayoutElement>();
                toggleLayoutElement.preferredHeight = height;
                toggleLayoutElement.minHeight = height;
            }

            if (ToggleGroupTagHandler.CurrentToggleGroupInstance != null)
            {
                var xmlLayoutToggleGroupInstance = ToggleGroupTagHandler.CurrentToggleGroupInstance;

                xmlLayoutToggleGroupInstance.AddToggle(toggle);
                xmlLayoutToggleGroupInstance.UpdateToggleElement(toggle);

                toggle.onValueChanged.AddListener((e) =>
                {
                    if (e)
                    {
                        var value = xmlLayoutToggleGroupInstance.GetValueForElement(toggle);
                        xmlLayoutToggleGroupInstance.SetSelectedValue(value);
                    }
                });
            }


            // Text attributes
            if (textComponent != null)
            {
                var tagHandler = XmlLayoutUtilities.GetXmlTagHandler("Text");
                tagHandler.SetInstance(textComponent.rectTransform, this.currentXmlLayoutInstance);

                var textAttributes = new AttributeDictionary(
                                            attributesToApply.Where(a => TextTagHandler.TextAttributes.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
                                                      .ToDictionary(a => a.Key, b => b.Value));

                if (attributesToApply.ContainsKey("textshadow")) textAttributes.Add("shadow", attributesToApply["textshadow"]);
                if (attributesToApply.ContainsKey("textoutline")) textAttributes.Add("outline", attributesToApply["textoutline"]);
                if (attributesToApply.ContainsKey("textcolor")) textAttributes.Add("color", attributesToApply["textcolor"]);

                tagHandler.ApplyAttributes(textAttributes);

                // disable the XmlElement component, it can interfere with mouse clicks/etc.
                textComponent.GetComponent<XmlElement>().enabled = false;

                // disable the text component if there is no text
                textComponent.gameObject.SetActive(!String.IsNullOrEmpty(textComponent.text));
            }

            if (!attributesToApply.ContainsKey("text") && !currentXmlElement.attributes.ContainsKey("text"))
            {
                var background = (Image)toggle.targetGraphic;
                var backgroundTransform = background.rectTransform;

                if (!attributesToApply.ContainsKey("dontModifyBackground"))
                {
                    backgroundTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    backgroundTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    backgroundTransform.anchoredPosition3D = new Vector3(0, 0, 0);
                }
            }

            if (attributesToApply.ContainsKey("togglebackgroundimage"))
            {
                targetGraphic.sprite = attributesToApply["togglebackgroundimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("togglecheckmarkimage"))
            {
                checkMark.sprite = attributesToApply["togglecheckmarkimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("togglecheckmarkimagepreserveaspect"))
            {
                checkMark.preserveAspect = attributesToApply["togglecheckmarkimagepreserveaspect"].ToBoolean();
            }

            if (attributesToApply.ContainsKey("togglecheckmarksize"))
            {
                float checkSize = float.Parse(attributesToApply["togglecheckmarksize"]);

                //var checkMarkLayoutElement = checkMark.GetComponent<LayoutElement>();
                //checkMarkLayoutElement.preferredWidth = checkSize;
                //checkMarkLayoutElement.preferredHeight = checkSize;

                checkMark.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                checkMark.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, checkSize);
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, checkSize);
            }
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var toggle = (Toggle)primaryComponent;
                        var transform = currentInstanceTransform;
                        var layout = currentXmlLayoutInstance;

                        var eventData = GetEventValueData(eventValue);

                        toggle.onValueChanged.AddListener((e) =>
                        {
                            string _value = eventData.value;
                            var valueLower = eventData.value.ToLower();

                            if (valueLower == "selectedvalue")
                            {
                                _value = e.ToString();
                            }

                            layout.XmlLayoutController.ReceiveMessage(eventData.methodName, _value, transform, eventData.parameters);
                        });
                    }
                    break;

                default:
                    base.HandleEventAttribute(eventName, eventValue);
                    break;
            }
        }

        public string GetValue(XmlElement element)
        {
            return element.GetComponent<Toggle>().isOn.ToString();
        }

        public override void SetValue(string newValue, bool triggerEventHandlers = true)
        {
            var toggle = currentXmlElement.GetComponent<Toggle>();
            var parsedValue = newValue.ToBoolean();

            // if the value isn't changing, don't execute anything
            if (toggle.isOn == parsedValue) return;

            var eventBackup = toggle.onValueChanged;

            if(!triggerEventHandlers) toggle.onValueChanged = new Toggle.ToggleEvent();

            toggle.isOn = parsedValue;

            if(!triggerEventHandlers) toggle.onValueChanged = eventBackup;
        }
    }
}
