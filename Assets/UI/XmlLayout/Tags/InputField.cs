using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.Events;

namespace UI.Xml.Tags
{
    public class InputFieldTagHandler : InputBaseTagHandler, IHasXmlFormValue
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<InputField>();
            }
        }

        private List<string> _eventAttributeNames = new List<string>()
        {
            "onClick",
            "onMouseEnter",
            "onMouseExit",
            "onValueChanged",
            "onEndEdit",
            "onSubmit",
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

            var inputField = primaryComponent as InputField;

            var textComponents = new List<Text> { inputField.textComponent };
            if (inputField.placeholder != null)
            {
                var placeholderText = inputField.placeholder.GetComponent<Text>();
                if (placeholderText != null)
                {
                    textComponents.Add(placeholderText);
                }

                if (attributesToApply.ContainsKey("placeholdertext"))
                {
                    placeholderText.text = StringExtensions.DecodeEncodedNonAsciiCharacters(attributesToApply["placeholdertext"]);
                }
            }

            foreach (var textComponent in textComponents)
            {
                var tagHandler = XmlLayoutUtilities.GetXmlTagHandler("Text");
                tagHandler.SetInstance(textComponent.rectTransform, this.currentXmlLayoutInstance);

                var textAttributes = new AttributeDictionary(
                                            attributesToApply.Where(a => TextTagHandler.TextAttributes.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
                                                      .ToDictionary(a => a.Key, b => b.Value));

                if (attributesToApply.ContainsKey("textshadow")) textAttributes.Add("shadow", attributesToApply["textshadow"]);
                if (attributesToApply.ContainsKey("textoutline")) textAttributes.Add("outline", attributesToApply["textoutline"]);
                if (attributesToApply.ContainsKey("textcolor")) textAttributes.Add("color", attributesToApply["textcolor"]);
                if (attributesToApply.ContainsKey("textalignment")) textAttributes.Add("alignment", attributesToApply["textalignment"]);

                textAttributes.Remove("text");

                if (attributesToApply.ContainsKey("textoffset"))
                {
                    var offset = attributesToApply["textoffset"].ToRectOffset();
                    textAttributes.Add("offsetMin", String.Format("{0} {1}", offset.left, offset.bottom));
                    textAttributes.Add("offsetMax", String.Format("-{0} -{1}", offset.right, offset.top));
                }

                tagHandler.ApplyAttributes(textAttributes);
            }
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                case "onendedit":
                case "onsubmit":
                    {
                        var inputField = (InputField)primaryComponent;
                        var transform = currentInstanceTransform;
                        var layout = currentXmlLayoutInstance;

                        var eventData = GetEventValueData(eventValue);

                        var listener = new UnityAction<string>(
                            (e) =>
                            {
                                string _value = eventData.value;

                                if (eventData.value != null)
                                {
                                    var valueLower = eventData.value.ToLower();

                                    if (valueLower == "value")
                                    {
                                        _value = e.ToString();
                                    }
                                }

                                layout.XmlLayoutController.ReceiveMessage(eventData.methodName, _value, transform, eventData.parameters);
                            });

                        if (eventName == "onvaluechanged")
                        {
                            inputField.onValueChanged.AddListener(listener);
                        }
                        else if (eventName == "onendedit")
                        {
                            inputField.onEndEdit.AddListener(listener);
                        }
                        else if (eventName == "onsubmit")
                        {
                            currentXmlElement.AddOnSubmitEvent(() =>
                            {
                                string _value = eventData.value;

                                if (eventData.value != null && eventData.value.ToLower() == "value") _value = inputField.text;

                                listener.Invoke(_value);
                            });
                        }
                    }
                    break;

                default:
                    base.HandleEventAttribute(eventName, eventValue);
                    break;
            }
        }

        public string GetValue(XmlElement element)
        {
            return element.GetComponent<InputField>().text;
        }
    }
}
