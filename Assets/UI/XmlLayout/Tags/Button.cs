using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UI.Tables;

#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UI.Xml.Tags
{
    public class ButtonTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Button>();
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            base.ApplyAttributes(attributesToApply);

            var xmlLayoutButton = currentInstanceTransform.GetComponent<XmlLayoutButton>();            

            var textComponent = currentInstanceTransform.GetComponentInChildren<Text>(true);
            bool applyTextAttributesFromButton = true;

            ColorBlock textColors = new ColorBlock() { normalColor = Color.black, highlightedColor = Color.black, disabledColor = Color.black, pressedColor = Color.black, colorMultiplier = 1 };

            if (attributesToApply.ContainsKey("textcolors"))
            {
                textColors = attributesToApply["textcolors"].ToColorBlock(currentXmlLayoutInstance);
            }
            else if (attributesToApply.ContainsKey("textcolor") || attributesToApply.ContainsKey("deselectedtextcolor"))
            {
                // draw the text colors from the textColor attribute
                var textColor = (attributesToApply.ContainsKey("textcolor") ? attributesToApply["textcolor"] : attributesToApply["deselectedtextcolor"]).ToColor(currentXmlLayoutInstance);

                SetColorBlockColor(ref textColors, textColor);
            }

            if (currentXmlElement.childElements.Count > 0)
            {
                var child = currentXmlElement.childElements.First();
                
                if (child.tagType == "Text")
                {
                    if (child.gameObject != textComponent.gameObject)
                    {
                        // Replace the original text component with the child element
                        child.rectTransform.SetParent(textComponent.rectTransform.parent);
                        if (Application.isPlaying)
                            GameObject.Destroy(textComponent.gameObject);
                        else
                            GameObject.DestroyImmediate(textComponent.gameObject);

                        textComponent = child.GetComponent<Text>();
                    }

                    applyTextAttributesFromButton = false;

                    if (child.attributes.ContainsKey("color"))
                    {
                        SetColorBlockColor(ref textColors, textComponent.color);
                    }

                    xmlLayoutButton.TextComponent = new TextComponentWrapper(textComponent);
                }
#if TEXTMESHPRO_PRESENT
                else if (child.tagType == "TextMeshPro")
                {                    
                    if (textComponent != null && child.gameObject != textComponent.gameObject)
                    {
                        // Replace the original text component with the child element
                        child.rectTransform.SetParent(textComponent.rectTransform.parent);
                        if (Application.isPlaying)
                            GameObject.Destroy(textComponent.gameObject);
                        else
                            GameObject.DestroyImmediate(textComponent.gameObject);                        
                    }

                    var textMeshPro = child.GetComponent<TextMeshProUGUI>();

                    applyTextAttributesFromButton = false;

                    if (child.attributes.ContainsKey("color"))
                    {
                        SetColorBlockColor(ref textColors, textMeshPro.color);
                    }

                    xmlLayoutButton.TextComponent = new TextComponentWrapper(textMeshPro);
                }
#endif

                if (!child.attributes.ContainsKey("text") && attributesToApply.ContainsKey("text"))
                {
                    child.SetAndApplyAttribute("text", attributesToApply["text"]);
                }

                child.rectTransform.localScale = Vector3.one;
            }
            else
            {
                xmlLayoutButton.TextComponent = new TextComponentWrapper(textComponent);
            }            

            if (applyTextAttributesFromButton)
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

                tagHandler.ApplyAttributes(textAttributes);
            }

            // preserve aspect for button background
            var imageComponent = currentInstanceTransform.GetComponent<Image>();
            if (attributesToApply.ContainsKey("preserveaspect")) imageComponent.preserveAspect = attributesToApply["preserveaspect"].ToBoolean();                      

            // Button image
            if (attributesToApply.ContainsKey("icon") || this.currentXmlElement.attributes.ContainsKey("icon"))
            {
                var cell = xmlLayoutButton.IconCell;
                
                // position the cell on the left or the right
                var alignmentParameter = attributesToApply.ContainsKey("iconAlignment")
                                       ? attributesToApply["iconAlignment"]
                                       : (currentXmlElement.attributes.ContainsKey("iconAlignment") ? currentXmlElement.attributes["iconAlignment"] : "Left");

                var imageAlignment = (ButtonIconAlignment)Enum.Parse(typeof(ButtonIconAlignment), alignmentParameter);

                var buttonImageWidth = attributesToApply.ContainsKey("iconwidth") 
                                        ? attributesToApply["iconwidth"].ToFloat() 
                                        : currentXmlElement.attributes.ContainsKey("iconwidth") ? currentXmlElement.attributes["iconwidth"].ToFloat() : 0;

                xmlLayoutButton.ButtonTableLayout.ColumnWidths = new List<float>() { 0, 0 };

                if (imageAlignment == ButtonIconAlignment.Left)
                {
                    cell.transform.SetAsFirstSibling();
                    xmlLayoutButton.ButtonTableLayout.ColumnWidths[0] = buttonImageWidth;
                }
                else
                {
                    cell.transform.SetAsLastSibling();
                    xmlLayoutButton.ButtonTableLayout.ColumnWidths[1] = buttonImageWidth;
                }

                xmlLayoutButton.IconComponent.preserveAspect = true;

                if (attributesToApply.ContainsKey("icon"))
                {
                    xmlLayoutButton.IconComponent.sprite = attributesToApply["icon"].ToSprite();
                }

                if (attributesToApply.ContainsKey("iconcolor"))
                {
                    xmlLayoutButton.IconColor = attributesToApply["iconcolor"].ToColor(currentXmlLayoutInstance);
                }

                if (attributesToApply.ContainsKey("iconhovercolor"))
                {
                    xmlLayoutButton.IconHoverColor = attributesToApply["iconhovercolor"].ToColor(currentXmlLayoutInstance);
                }

                if (attributesToApply.ContainsKey("icondisabledcolor"))
                {
                    xmlLayoutButton.IconDisabledColor = attributesToApply["icondisabledcolor"].ToColor(currentXmlLayoutInstance);
                }

                if (attributesToApply.ContainsKey("iconimagetype"))
                {                    
                    xmlLayoutButton.IconComponent.type = (Image.Type)Enum.Parse(typeof(Image.Type), attributesToApply["iconimagetype"]);
                }

                cell.gameObject.SetActive(true);

                // show or hide the text cell if there is (or is not) any text
                if ((!attributesToApply.ContainsKey("text") || String.IsNullOrEmpty(attributesToApply["text"]))
                  && !currentXmlElement.attributes.ContainsKey("text"))
                {
                    xmlLayoutButton.TextCell.gameObject.SetActive(false);
                }
                else
                {
                    xmlLayoutButton.TextCell.gameObject.SetActive(true);
                }
            }

            if (attributesToApply.ContainsKey("padding"))
            {
                xmlLayoutButton.ButtonTableLayout.padding = attributesToApply["padding"].ToRectOffset();
            }
                
            xmlLayoutButton.TextColors = textColors;

            XmlLayoutTimer.DelayedCall(0,
            () =>
            {
                if (xmlLayoutButton.mouseIsOver)                
                    xmlLayoutButton.OnPointerEnter(null);                
                else                
                    xmlLayoutButton.OnPointerExit(null);
                
            }, xmlLayoutButton);
        }

        void SetColorBlockColor(ref ColorBlock block, Color color)
        {
            block.normalColor = color;
            block.highlightedColor = color;
            block.disabledColor = color;
            block.pressedColor = color;
            block.colorMultiplier = 1;
        }

        protected override AttributeDictionary defaultAttributeValues
        {
            get
            {
                return new AttributeDictionary()
                {
                    {"interactable", "true"}
                };
            }
        }
    }
}
