#if TEXTMESHPRO_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.Xml;
using TMPro;

namespace UI.Xml.Tags
{
    public partial class TextMeshProDropdownTagHandler : ElementTagHandler, IHasXmlFormValue
    {
        public static TextMeshProDropdownTagHandler CurrentHandler { get; private set; }

        public override bool isCustomElement { get { return true; } }
        public override string prefabPath { get { return "XmlLayout Prefabs/TextMeshPro/TextMeshPro - Dropdown"; } }

        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentXmlElement.GetComponent<TMP_Dropdown>();
            }
        }

        public TMP_Dropdown CurrentDropdown
        {
            get
            {
                return primaryComponent as TMP_Dropdown;
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

        public override void Open(AttributeDictionary elementAttributes)
        {
            base.Open(elementAttributes);

            currentInstanceTransform.name = "TextMeshPro - Dropdown";
            CurrentHandler = this;

            var tmpDropdown = currentInstanceTransform.GetComponent<TMP_Dropdown>();
            if (tmpDropdown == null)
            {
                tmpDropdown = currentInstanceTransform.gameObject.AddComponent<TMP_Dropdown>();
                tmpDropdown.ClearOptions();
            }

            tmpDropdown.targetGraphic = tmpDropdown.GetComponent<Image>();

            var labelTransform = currentInstanceTransform.Find("Label") as RectTransform;
            var label = labelTransform.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
                label.color = new Color(0.2f, 0.2f, 0.2f);
                label.fontSize = 14;
                label.alignment = TextAlignmentOptions.Left;
            }

            var dropdownTransform = currentXmlElement.rectTransform.Find("Template") as RectTransform;

            // if we don't make the template active while we set the dimensions, then TextMesh Pro will override them later
            dropdownTransform.gameObject.SetActive(true);
            var itemLabelTransform = currentInstanceTransform.Find("Template/Viewport/Content/Item/Item Label") as RectTransform;

            var itemLabel = itemLabelTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (itemLabel == null)
            {
                itemLabel = itemLabelTransform.gameObject.AddComponent<TextMeshProUGUI>();
                itemLabel.alignment = TextAlignmentOptions.Left;
                itemLabel.color = new Color(0.2f, 0.2f, 0.2f);
                itemLabel.fontSize = 14;
            }

            tmpDropdown.template = dropdownTransform;
            tmpDropdown.captionText = label;
            tmpDropdown.itemText = itemLabel;

            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = new Vector2(10, 6);
            labelTransform.offsetMax = new Vector2(-25, -7);

            itemLabelTransform.anchorMin = Vector2.zero;
            itemLabelTransform.anchorMax = Vector2.one;
            itemLabelTransform.offsetMin = new Vector2(20, 1);
            itemLabelTransform.offsetMax = new Vector2(-20, -2);

            var itemLabelXmlElement = itemLabelTransform.GetComponent<XmlElement>() ?? itemLabelTransform.gameObject.AddComponent<XmlElement>();
            itemLabelXmlElement.SetAttribute("alignment", "MidlineLeft");
            itemLabelXmlElement.SetAttribute("offsetMin", "20,1");
            itemLabelXmlElement.SetAttribute("offsetMax", "-20,-2");

            // disable the template again
            dropdownTransform.gameObject.SetActive(false);
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            var dropdownTransform = currentXmlElement.rectTransform.Find("Template") as RectTransform;

            var layoutElement = currentInstanceTransform.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = currentInstanceTransform.gameObject.AddComponent<LayoutElement>();

            // apply attributes as per usual
            base.ApplyAttributes(attributesToApply);

            if (ElementHasAttribute("dropdownheight", attributesToApply))
            {
                dropdownTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, float.Parse(attributesToApply["dropdownheight"]));
            }

            var itemTemplate = CurrentDropdown.itemText.rectTransform.parent as RectTransform;

            if (ElementHasAttribute("itemHeight", attributesToApply))
            {
                var itemHeight = float.Parse(currentXmlElement.GetAttribute("itemheight"));
                (itemTemplate.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);

                // it's also necessary to set the height of the content transform, otherwise we end up with weird issues
                var contentTransform = currentXmlElement.rectTransform.Find("Template/Viewport/Content") as RectTransform;

                contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);
            }

            if (attributesToApply.ContainsKey("itemWidth"))
            {
                var itemWidth = attributesToApply["itemWidth"].ToFloat();
                (itemTemplate.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemWidth);
            }

            var arrow = currentXmlElement.rectTransform.Find("Arrow").GetComponent<Image>();
            if(attributesToApply.ContainsKey("arrowImage"))
                arrow.sprite = attributesToApply["arrowImage"].ToSprite();
            if (attributesToApply.ContainsKey("arrowColor"))
                arrow.color = attributesToApply["arrowColor"].ToColor(currentXmlLayoutInstance);

            if (attributesToApply.ContainsKey("arrowOffset"))
            {
                arrow.rectTransform.anchoredPosition = attributesToApply["arrowOffset"].ToVector2();
            }

            if (attributesToApply.ContainsKey("itemBackgroundColors"))
            {
                var toggle = itemTemplate.GetComponent<Toggle>();
                toggle.colors = attributesToApply["itemBackgroundColors"].ToColorBlock(currentXmlLayoutInstance);
            }

            var dropdownBackground = dropdownTransform.GetComponent<Image>();
            if (attributesToApply.ContainsKey("dropdownBackgroundColor"))
                dropdownBackground.color = attributesToApply["dropdownBackgroundColor"].ToColor(currentXmlLayoutInstance);
            if(attributesToApply.ContainsKey("dropdownBackgroundImage"))
                dropdownBackground.sprite = attributesToApply["dropdownBackgroundImage"].ToSprite();

            var scrollbar = dropdownTransform.GetComponentInChildren<Scrollbar>();
            var scrollbarImage = scrollbar.targetGraphic as Image;
            if (attributesToApply.ContainsKey("scrollbarColors"))
                scrollbar.colors = attributesToApply["scrollbarColors"].ToColorBlock(currentXmlLayoutInstance);
            if (attributesToApply.ContainsKey("scrollbarImage"))
                scrollbarImage.sprite = attributesToApply["scrollbarImage"].ToSprite();

            var scrollbarBackground = scrollbar.GetComponent<Image>();
            if (attributesToApply.ContainsKey("scrollbarBackgroundColor"))
                scrollbarBackground.color = attributesToApply["scrollbarBackgroundColor"].ToColor(currentXmlLayoutInstance);
            if (attributesToApply.ContainsKey("scrollbarBackgroundImage"))
                scrollbarBackground.sprite = attributesToApply["scrollbarBackgroundImage"].ToSprite();

            if (attributesToApply.ContainsKey("scrollbarWidth"))
                scrollbar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attributesToApply["scrollbarWidth"].ToFloat());

            if (attributesToApply.ContainsKey("padding"))
            {
                var padding = attributesToApply["padding"].ToVector4();

                var itemLabelTransform = currentInstanceTransform.Find("Label") as RectTransform;
                itemLabelTransform.offsetMin = new Vector2(padding.x, padding.w);
                itemLabelTransform.offsetMax = new Vector2(-padding.y, -padding.z);
            }

            var checkMark = itemTemplate.Find("Item Checkmark").GetComponent<Image>();

            if (attributesToApply.ContainsKey("checkColor"))
            {
                checkMark.color = attributesToApply["checkColor"].ToColor(currentXmlLayoutInstance);
            }
            else
            {
                checkMark.color = new Color(0, 0, 0);
            }

            if (attributesToApply.ContainsKey("checkImage"))
            {
                checkMark.sprite = attributesToApply["checkImage"].ToSprite();
            }
            else
            {
                checkMark.sprite = XmlLayoutUtilities.LoadResource<Sprite>("Sprites/Elements/Checkmark");
            }

            if (attributesToApply.ContainsKey("checkSize"))
            {
                var size = attributesToApply["checkSize"].ToFloat();
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                checkMark.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }

            if (attributesToApply.ContainsKey("checkImagePreserveAspect"))
                checkMark.preserveAspect = attributesToApply["checkMarkImagePreserveAspect"].ToBoolean();

            // data source
#if !ENABLE_IL2CPP && MVVM_ENABLED
            if (attributesToApply.ContainsKey("vm-options"))
            {
                var xmlLayoutDropdown = currentXmlElement.GetComponent<XmlLayoutDropdown>();
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

            CurrentDropdown.captionText.raycastTarget = false;
            CurrentDropdown.itemText.raycastTarget = false;
            CurrentDropdown.RefreshShownValue();

            CurrentHandler = null;
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"interactable", "xs:boolean"},
                    {"arrowImage", "xs:string"},
                    {"arrowColor", "xmlLayout:color"},
                    {"arrowOffset", "xmlLayout:vector2" },
                    {"dropDownHeight", "xs:float"},
                    {"itemHeight", "xs:float"},
                    {"itemWidth", "xs:float" },
                    {"checkColor", "xmlLayout:color"},
                    {"checkSize", "xs:float"},
                    {"checkImage", "xs:string"},
                    {"checkImagePreserveAspect", "xs:boolean"},

                    {"itemBackgroundColors", "xmlLayout:colorblock"},

                    {"dropDownBackgroundColor", "xmlLayout:color"},
                    {"dropDownBackgroundImage", "xs:string"},

                    {"scrollbarColors", "xmlLayout:colorblock"},
                    {"scrollbarImage", "xs:string"},
                    {"scrollbarBackgroundColor", "xmlLayout:color"},
                    {"scrollbarBackgroundImage", "xs:string"},
                    {"scrollbarWidth", "xs:float"},

                    {"colors", "xmlLayout:colorblock"},
                    {"vm-options", "xs:string"},

                    {"onValueChanged", "xmlLayout:function"},
                    {"padding", "xmlLayout:vector4"}
                };
            }
        }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "image",
                };
            }
        }

        public override string elementChildType
        {
            get
            {
                return "TextMeshProDropdown";
            }
        }

        public string GetValue(XmlElement element)
        {
            var dropdown = element.GetComponent<TMP_Dropdown>();
            return dropdown.options[dropdown.value].text;
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            if (String.IsNullOrEmpty(newValue)) return;

            var dropdown = (TMP_Dropdown)primaryComponent;

            int selectedValue = -1;

            var eventBackup = dropdown.onValueChanged;
            if (!fireEventHandlers) dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();

            if (int.TryParse(newValue, out selectedValue))
            {
                // if the value is an integer, use the option index
                dropdown.value = selectedValue;
                dropdown.RefreshShownValue();
            }
            else
            {
                // otherwise, try and find an option with matching text
                var option = dropdown.options.FirstOrDefault(o => o.text.Equals(newValue, StringComparison.OrdinalIgnoreCase));

                if (option != null)
                {
                    dropdown.value = dropdown.options.IndexOf(option);
                    dropdown.RefreshShownValue();
                }
            }

            if (!fireEventHandlers) dropdown.onValueChanged = eventBackup;
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {
            switch (eventName)
            {
                case "onvaluechanged":
                    {
                        var dropdown = (TMP_Dropdown)primaryComponent;
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
    }

    [ElementTagHandler("TMP_Option")]
    public class TextMeshProDropdownOption : ElementTagHandler
    {
        public override bool isCustomElement { get { return true; } }
        public override string prefabPath { get { return null; } }
        public override string elementGroup { get { return "TextMeshProDropdown"; } }

        public override bool renderElement { get { return false; } }

        public override string extension { get { return "blank"; } }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // this should never happen, but just in case
            if (TextMeshProDropdownTagHandler.CurrentHandler == null) return;

            var text = ElementHasAttribute("text", attributesToApply) ? attributesToApply.GetValue("text") : string.Empty;
            var selected = ElementHasAttribute("selected", attributesToApply) ? currentXmlElement.GetAttribute("selected").ToBoolean() : false;

            var dropdown = TextMeshProDropdownTagHandler.CurrentHandler.CurrentDropdown;

            var optionData = new TMP_Dropdown.OptionData { text = text };

            dropdown.options.Add(optionData);
            if (selected) dropdown.value = dropdown.options.IndexOf(optionData);
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"selected", "xs:boolean"},
                    {"text", "xs:string"}
                };
            }
        }
    }

    [ElementTagHandler("TMP_DropdownLabel")]
    public class TextMeshProDropdownLabel : TextMeshProTagHandler
    {
        public override string elementGroup { get { return "TextMeshProDropdown"; } }
        public override bool renderElement { get { return false; } }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // this should never happen, but just in case
            if (TextMeshProDropdownTagHandler.CurrentHandler == null) return;

            attributesToApply.AddIfKeyNotExists("alignment", "Left");
            attributesToApply.AddIfKeyNotExists("dontMatchParentDimensions", "true");

            var dropdown = TextMeshProDropdownTagHandler.CurrentHandler.CurrentDropdown;

            var TMPHandler = XmlLayoutUtilities.GetXmlTagHandler("TextMeshPro");

            TMPHandler.SetInstance(dropdown.captionText.rectTransform, currentXmlLayoutInstance);
            TMPHandler.ApplyAttributes(attributesToApply);

            var xmlElement = dropdown.captionText.GetComponent<XmlElement>();
            //xmlElement.attributes = xmlElement.attributes.Merge(attributesToApply);
            xmlElement.attributes.Merge(attributesToApply);
        }
    }

    [ElementTagHandler("TMP_OptionTextTemplate")]
    public class TextMeshProDropdownOptionTextTemplate : TextMeshProTagHandler
    {
        public override string elementGroup { get { return "TextMeshProDropdown"; } }
        public override bool renderElement { get { return false; } }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // this should never happen, but just in case
            if (TextMeshProDropdownTagHandler.CurrentHandler == null) return;

            attributesToApply.AddIfKeyNotExists("alignment", "Left");
            attributesToApply.AddIfKeyNotExists("dontMatchParentDimensions", "true");

            var dropdown = TextMeshProDropdownTagHandler.CurrentHandler.CurrentDropdown;

            var TMPHandler = XmlLayoutUtilities.GetXmlTagHandler("TextMeshPro");

            TMPHandler.SetInstance(dropdown.itemText.rectTransform, currentXmlLayoutInstance);
            TMPHandler.ApplyAttributes(attributesToApply);

            if (attributesToApply.ContainsKey("padding"))
            {
                var padding = attributesToApply["padding"].ToRectOffset();
                dropdown.itemText.rectTransform.offsetMin = new Vector2(padding.left, padding.bottom);
                dropdown.itemText.rectTransform.offsetMax = new Vector2(-padding.right, -padding.top);
            }

            var xmlElement = dropdown.itemText.GetComponent<XmlElement>();
            //xmlElement.attributes = xmlElement.attributes.Merge(attributesToApply);
            xmlElement.attributes.Merge(attributesToApply);
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                var attr = base.attributes;

                attr.AddIfKeyNotExists("padding", "xmlLayout:rectOffset");

                return attr;
            }
        }
    }
}
#endif
