using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

namespace UI.Xml.Tags
{
    public class ToggleGroupTagHandler : ElementTagHandler, IHasXmlFormValue
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlLayoutToggleGroup>();
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

        private static XmlLayoutToggleGroup previousToggleGroupInstance = null;
        public static XmlLayoutToggleGroup CurrentToggleGroupInstance;

        public override void Open(AttributeDictionary elementAttributes)
        {
            base.Open(elementAttributes);

            previousToggleGroupInstance = CurrentToggleGroupInstance;
            CurrentToggleGroupInstance = this.primaryComponent as XmlLayoutToggleGroup;
        }

        public override void Close()
        {
            base.Close();

            //CurrentToggleGroupInstance.SetSelectedValue(CurrentToggleGroupInstance.GetSelectedValue(), false);

            CurrentToggleGroupInstance = previousToggleGroupInstance;
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var xmlLayoutToggleGroup = (XmlLayoutToggleGroup)primaryComponent;
                        var transform = currentInstanceTransform;

                        var eventData = GetEventValueData(eventValue);

                        xmlLayoutToggleGroup.AddOnValueChangedEventHandler((int e) =>
                            {
                                string _value = eventData.value;
                                var valueLower = eventData.value.ToLower();

                                if (valueLower == "selectedvalue")
                                {
                                    _value = e.ToString();
                                }
                                else if (valueLower == "selectedtext")
                                {
                                    _value = xmlLayoutToggleGroup.GetTextValueForIndex(e);
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

        public string GetValue(XmlElement element)
        {
            return element.GetComponent<XmlLayoutToggleGroup>().GetSelectedValue().ToString();
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            (primaryComponent as XmlLayoutToggleGroup).SetSelectedValue(newValue, fireEventHandlers);
        }

#if !ENABLE_IL2CPP && MVVM_ENABLED
        protected override void EnableTwoWayBinding(string dataSource)
        {
            var controller = (XmlLayoutControllerMVVM)currentXmlLayoutInstance.XmlLayoutController;
            var toggleGroup = primaryComponent as XmlLayoutToggleGroup;

            toggleGroup.AddOnValueChangedEventHandler(
                (string s) =>
                {
                    controller.SetViewModelValue(dataSource, toggleGroup.GetSelectedTextValue(), true);
                });
        }
#endif
    }
}
