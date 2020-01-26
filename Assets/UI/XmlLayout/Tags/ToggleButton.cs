using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.Xml;

namespace UI.Xml.Tags
{
    public class ToggleButtonTagHandler : ButtonTagHandler, IHasXmlFormValue
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlLayoutToggleButton>();
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

            var toggleComponent = currentInstanceTransform.GetComponent<Toggle>();

            if (attributesToApply.ContainsKey("colors"))
            {
                toggleComponent.colors = attributesToApply["colors"].ToColorBlock(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("ison"))
            {
                toggleComponent.isOn = attributesToApply["ison"].ToBoolean();
            }

            if (attributesToApply.ContainsKey("selectedicon"))
            {
                var xmlLayoutToggleButton =toggleComponent.GetComponent<XmlLayoutToggleButton>();
                xmlLayoutToggleButton.SelectedIconSprite = attributesToApply["selectedicon"].ToSprite();
                xmlLayoutToggleButton.DeselectedIconSprite = xmlLayoutToggleButton.IconComponent.sprite;
            }

            if (ToggleGroupTagHandler.CurrentToggleGroupInstance != null)
            {
                var xmlLayoutToggleGroupInstance = ToggleGroupTagHandler.CurrentToggleGroupInstance;

                xmlLayoutToggleGroupInstance.AddToggle(toggleComponent);
                xmlLayoutToggleGroupInstance.UpdateToggleElement(toggleComponent);

                toggleComponent.onValueChanged.AddListener((e) =>
                {
                    if (e)
                    {
                        var value = xmlLayoutToggleGroupInstance.GetValueForElement(toggleComponent);
                        xmlLayoutToggleGroupInstance.SetSelectedValue(value);
                    }
                });
            }

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                var xmlLayoutToggleButton = toggleComponent.GetComponent<XmlLayoutToggleButton>();
                xmlLayoutToggleButton.UpdateDisplay();
            }, toggleComponent);
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var toggle = primaryComponent.GetComponent<Toggle>();
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

            if (!triggerEventHandlers) toggle.onValueChanged = new Toggle.ToggleEvent();

            toggle.isOn = parsedValue;

            if (!triggerEventHandlers) toggle.onValueChanged = eventBackup;
        }
    }
}
