using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

namespace UI.Xml.Tags
{
    public partial class DropdownTagHandler : ElementTagHandler, IHasXmlFormValue
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Dropdown>();
            }
        }

        private List<string> _eventAttributeNames = new List<string>()
        {
            "onClick",
            "onMouseEnter",
            "onMouseExit",
            "onValueChanged"
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

            var dropdown = primaryComponent as Dropdown;
            var templateComponent = dropdown.template.GetComponent<Image>();
            var itemTemplateComponent = dropdown.template.GetComponentInChildren<Toggle>();
            var textComponent = dropdown.captionText;

            var textTagHandler = XmlLayoutUtilities.GetXmlTagHandler("Text");
            textTagHandler.SetInstance(textComponent.rectTransform, this.currentXmlLayoutInstance);

            var textAttributes = new AttributeDictionary(
                                            attributesToApply.Where(a => TextTagHandler.TextAttributes.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
                                                      .ToDictionary(a => a.Key, b => b.Value));

            if (attributesToApply.ContainsKey("textshadow")) textAttributes.Add("shadow", attributesToApply["textshadow"]);
            if (attributesToApply.ContainsKey("textoutline")) textAttributes.Add("outline", attributesToApply["textoutline"]);
            if (attributesToApply.ContainsKey("textcolor")) textAttributes.Add("color", attributesToApply["textcolor"]);
            if (attributesToApply.ContainsKey("textalignment")) textAttributes.Add("alignment", attributesToApply["textalignment"]);

            textTagHandler.ApplyAttributes(textAttributes);
            // disable the XmlElement component, it can interfere with mouse clicks/etc.
            textComponent.GetComponent<XmlElement>().enabled = false;

            var xmlLayoutDropdown = dropdown.GetComponent<XmlLayoutDropdown>();
            var arrow = xmlLayoutDropdown.Arrow;
            if (attributesToApply.ContainsKey("arrowimage"))
            {
                arrow.sprite = attributesToApply["arrowimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("arrowcolor"))
            {
                arrow.color = attributesToApply["arrowcolor"].ToColor(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("arrowOffset"))
            {
                arrow.rectTransform.anchoredPosition = attributesToApply["arrowOffset"].ToVector2();
            }

            // dropdownHeight property
            if (attributesToApply.ContainsKey("dropdownheight"))
            {
                dropdown.template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, float.Parse(attributesToApply["dropdownheight"]));
            }

            // Apply text attributes to the item template
            var itemTemplate = xmlLayoutDropdown.ItemTemplate;
            var itemTagHandler = XmlLayoutUtilities.GetXmlTagHandler("Toggle");
            var itemAttributes = attributesToApply.Clone();

            if (attributesToApply.ContainsKey("itemheight"))
            {
                var itemHeight = float.Parse(attributesToApply["itemheight"]);
                (itemTemplate.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);

                // it's also necessary to set the height of the content transform, otherwise we end up with weird issues
                var contentTransform = templateComponent.GetComponentInChildren<ScrollRect>().content;

                contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);
            }

            /*
            // This isn't working yet - it looks like the Dropdown code may be overriding this value
            if (attributesToApply.ContainsKey("itemWidth"))
            {
                var itemWidth = attributesToApply["itemWidth"].ToFloat();
                (itemTemplate.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemWidth);
            }
            */

            if (attributesToApply.ContainsKey("checkcolor"))
            {
                itemAttributes.Add("togglecheckmarkcolor", attributesToApply["checkcolor"]);
            }

            if (attributesToApply.ContainsKey("checksize"))
            {
                itemAttributes.Add("togglecheckmarksize", attributesToApply["checksize"]);
            }

            if (attributesToApply.ContainsKey("checkimage"))
            {
                itemAttributes.Add("togglecheckmarkimage", attributesToApply["checkimage"]);
            }

            if (attributesToApply.ContainsKey("checkimagepreserveaspect"))
            {
                itemAttributes.Add("togglecheckmarkimagepreserveaspect", attributesToApply["checkimagepreserveaspect"]);
            }

            // don't attempt to apply data source attributes to the item template
            itemAttributes.Remove("vm-dataSource");
            itemAttributes.Remove("vm-options");

            itemAttributes.Remove("color");
            itemAttributes.Remove("colors");

            // this attribute is checked by the Toggle to see if changes to the background should be permitted
            // (used by regular toggles to center the background if there is no text)
            itemAttributes.Add("dontModifyBackground", String.Empty);

            var itemXmlElement = itemTemplate.transform.GetComponent<XmlElement>();
            if (itemXmlElement == null)
            {
                itemTagHandler.SetInstance(itemTemplate.transform as RectTransform, this.currentXmlLayoutInstance);
                itemTagHandler.ApplyAttributes(itemAttributes);

                itemXmlElement = itemTemplate.transform.GetComponent<XmlElement>();
            }
            else
            {
                itemXmlElement.ApplyAttributes(itemAttributes);
            }

            if (itemXmlElement != null) itemXmlElement.enabled = false;

            if (attributesToApply.ContainsKey("itembackgroundcolors"))
            {
                itemTemplateComponent.colors = attributesToApply["itembackgroundcolors"].ToColorBlock(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("dropdownbackgroundcolor"))
            {
                dropdown.template.GetComponent<Image>().color = attributesToApply["dropdownbackgroundcolor"].ToColor(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("dropdownbackgroundimage"))
            {
                dropdown.template.GetComponent<Image>().sprite = attributesToApply["dropdownbackgroundimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("itemtextcolor"))
            {
                var itemTextComponent = dropdown.itemText;
                itemTextComponent.color = attributesToApply["itemtextcolor"].ToColor(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("scrollbarcolors"))
            {
                xmlLayoutDropdown.DropdownScrollbar.colors = attributesToApply["scrollbarcolors"].ToColorBlock(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("scrollbarimage"))
            {
                xmlLayoutDropdown.DropdownScrollbar.image.sprite = attributesToApply["scrollbarimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("scrollbarbackgroundcolor"))
            {
                xmlLayoutDropdown.DropdownScrollbar.GetComponent<Image>().color = attributesToApply["scrollbarbackgroundcolor"].ToColor(currentXmlLayoutInstance);
            }

            if (attributesToApply.ContainsKey("scrollbarbackgroundimage"))
            {
                xmlLayoutDropdown.DropdownScrollbar.GetComponent<Image>().sprite = attributesToApply["scrollbarbackgroundimage"].ToSprite();
            }

            if (attributesToApply.ContainsKey("scrollsensitivity"))
            {
                dropdown.template.GetComponent<ScrollRect>().scrollSensitivity = attributesToApply["scrollsensitivity"].ToFloat();
            }

            foreach (var attribute in attributesToApply)
            {
                SetPropertyValue(templateComponent, attribute.Key, attribute.Value);
            }

            // data source
#if !ENABLE_IL2CPP && MVVM_ENABLED
            if (attributesToApply.ContainsKey("vm-options"))
            {
                xmlLayoutDropdown.optionsDataSource = attributesToApply["vm-options"];
            }

            if (attributesToApply.ContainsKey("vm-dataSource"))
            {
                HandleDataSourceAttribute(attributesToApply.GetValue("vm-dataSource"), attributesToApply.GetValue("vm-options"));
            }
#endif
        }

        public override void Close()
        {
            base.Close();

            (primaryComponent as Dropdown).RefreshShownValue();
        }

        public override bool UseParseChildElements { get { return true; } }

        public override void ParseChildElements(XmlNode xmlNode)
        {
            var dropdown = (Dropdown)primaryComponent;
            dropdown.value = 0;

            int x = 0;
            int value = -1;
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name.ToLower() != "option") continue;

                var optionName = node.InnerText;

                dropdown.options.Add(new Dropdown.OptionData { text = optionName });

                var attributes = node.Attributes.ToAttributeDictionary();

                if (attributes.ContainsKey("selected"))
                {
                    try
                    {
                        if (attributes["selected"].ToBoolean())
                        {
                            value = x;
                        }
                    }
                    catch { }
                }

                x++;
            }

            if (value >= 0)
            {
                dropdown.value = value;
                dropdown.RefreshShownValue();
            }
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var dropdown = (Dropdown)primaryComponent;
                        var transform = currentInstanceTransform;
                        var layout = currentXmlLayoutInstance;

                        var eventData = GetEventValueData(eventValue);

                        dropdown.onValueChanged.AddListener((e) =>
                        {
                            string _value = eventData.value;
                            if (eventData.value != null)
                            {
                                var valueLower = eventData.value.ToLower();

                                if (valueLower == "selectedtext" || valueLower == "selectedvalue")
                                {
                                    _value = dropdown.options[e].text;
                                }
                                else if (valueLower == "selectedindex")
                                {
                                    _value = e.ToString();
                                }
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

        /// <summary>
        /// Get the text of the currently selected option
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetValue(XmlElement element)
        {
            var dropDown = element.GetComponent<Dropdown>();

            return dropDown.options[dropDown.value].text;
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            if (String.IsNullOrEmpty(newValue)) return;

            var dropdown = (Dropdown)primaryComponent;

            int selectedValue = -1;

            var eventBackup = dropdown.onValueChanged;
            if (!fireEventHandlers) dropdown.onValueChanged = new Dropdown.DropdownEvent();

            if (int.TryParse(newValue, out selectedValue))
            {
                // if the value is an integer, use the option index
                dropdown.SetSelectedValue(selectedValue);
            }
            else
            {
                // otherwise, try and find an option with matching text
                dropdown.SetSelectedValue(newValue);
            }

            if (!fireEventHandlers) dropdown.onValueChanged = eventBackup;
        }
    }
}
