#if TEXTMESHPRO_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Xml;
using TMPro;

namespace UI.Xml.Tags
{
    public class TextMeshProTagHandler : ElementTagHandler
    {
        static TextMeshProTagHandler()
        {
            RegisterCustomTypeHandlers();
        }

        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        public override string prefabPath
        {
            get
            {
                return null;
                //return "XmlLayout Prefabs/TextMeshPro/TextMeshPro";
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            if (currentInstanceTransform == null) return;

            if (currentXmlElement.name == "GameObject" || currentXmlElement.name == "Xml Element") currentXmlElement.name = "TextMesh Pro";

            var go = currentInstanceTransform.gameObject;

            //var mt = new MethodTimer("TMP::Get/Add Component");
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            //mt.Dispose();

            // If we don't add a LayoutElement component, TextMeshPro will use up any layout parameters (as it uses the ILayoutElement interface)
            // and it doesn't use them in the same way as a LayoutElement so things like, for example, 'flexibleWidth' will not work as expected
            // by adding a LayoutElement, we ensure that the TMP element responds to layout attributes in the same way as other elements
            var layoutElement = tmp.GetComponent<LayoutElement>();
            if (layoutElement == null) tmp.gameObject.AddComponent<LayoutElement>();

            if (!attributesToApply.ContainsKey("dontMatchParentDimensions") && !currentXmlElement.HasAttribute("delayedProcessingScheduled")) MatchParentDimensions();

            // default alignment, as per standard UI text
            if (!attributesToApply.ContainsKey("alignment") && !currentXmlElement.attributes.ContainsKey("alignment"))
            {
                tmp.alignment = TextAlignmentOptions.Center;
            }

            // default font size, as per standard UI text
            if (!attributesToApply.ContainsKey("fontSize") && !currentXmlElement.attributes.ContainsKey("fontSize"))
            {
                tmp.fontSize = 14f;
            }

            Material fontMaterial = null;
            if (attributesToApply.ContainsKey("fontMaterial"))
            {
                if (currentXmlLayoutInstance.textMeshProMaterials.ContainsKey(attributesToApply["fontMaterial"]))
                {
                    fontMaterial = currentXmlLayoutInstance.textMeshProMaterials[attributesToApply["fontMaterial"]];
                    attributesToApply.Remove("fontMaterial");
                }
            }

            //using (new MethodTimer("TMP::Base::ApplyAttributes"))
            {
                base.ApplyAttributes(attributesToApply);
            }

            if (fontMaterial != null)
            {
                tmp.fontMaterial = fontMaterial;
            }

            if (attributesToApply.ContainsKey("colorGradient"))
            {
                tmp.enableVertexGradient = true;
            }

            if (attributesToApply.ContainsKey("text"))
            {
                tmp.text = StringExtensions.DecodeEncodedNonAsciiCharacters(attributesToApply["text"]);
            }

            // For some reason, some TMP properties (such as text)
            // will not accept changes until after some internal TMP setup
            // has been completed. As such, it is necessary to delay our processing briefly
            // and then attempt to apply attributes again.
            // This only needs to be done once, as further ApplyAttributes() calls
            // will happen after TMP has already completed its setup.
            if (!currentXmlElement.HasAttribute("delayedProcessingScheduled"))
            {
                var attributeDictionaryCopy = attributesToApply;
                var _currentElement = currentXmlElement;
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    if (_currentElement != null)
                    {
                        if (_currentElement.gameObject.activeInHierarchy)
                        {
                            _currentElement.ApplyAttributes();
                        }
                        else
                        {
                            _currentElement.m_onEnableEventsOnceOff.Enqueue(() =>
                            {
                                _currentElement.ApplyAttributes(attributeDictionaryCopy);
                            });
                        }
                    }
                }, _currentElement, true);
                currentXmlElement.attributes.AddIfKeyNotExists("delayedProcessingScheduled", "true");
            }
            else
            {
                // TextMeshPro seems to sometimes ignore changes to the color property;
                // calling it again sorts this out
                if (attributesToApply.ContainsKey("color") && !attributesToApply.ContainsKey("delayedColorSet"))
                {
                    var _currentElement = currentXmlElement;
                    XmlLayoutTimer.AtEndOfFrame(() =>
                    {
                        _currentElement.ApplyAttributes(new Dictionary<string, string>()
                        {
                            {"color", attributesToApply["color"]},
                            {"delayedColorSet", "1"}
                        });

                        _currentElement.RemoveAttribute("delayedColorSet");
                    }, _currentElement, true);
                }
            }
        }

        /*public override void Close()
        {
            var tmp = (primaryComponent as TextMeshProUGUI);

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                // if we don't do this again, some attributes like outline/etc. seem to be lost
                ApplyAttributes(currentXmlElement.attributes);
            }, tmp);
        }*/

        public override bool isCustomElement
        {
            get
            {
                return true;
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                var options = new Dictionary<string, string>()
                {
                    {"text", "xs:string"},
                    {"font", "xs:string"},
                    {"fontStyle", "xs:string"},
                    {"fontSize", "xs:float"},
                    {"fontWeight", "xs:int"},
                    {"fontSizeMin", "xs:float"},
                    {"fontSizeMax", "xs:float"},
                    {"fontScale", "xs:float"},
                    {"enableAutoSizing", "xs:boolean"},
                    {"characterSpacing", "xs:float"},
                    {"characterWidthAdjustment", "xs:float"},
                    {"alpha", "xs:float"},
                    {"autoSizeTextContainer", "xs:boolean"},
                    {"color", "xmlLayout:color"},
                    {"faceColor", "xmlLayout:color"},
                    {"outlineColor", "xmlLayout:color"},
                    {"outlineWidth", "xs:float"},
                    {"fontMaterial", "xs:string"},
                    {"enableWordWrapping", "xs:boolean"},
                    {"wordWrappingRatios", "xs:float"},
                    {"extraPadding", "xs:boolean"},

                    {"wordSpacing", "xs:float"},
                    {"lineSpacing", "xs:float"},
                    {"lineSpacingAdjustment", "xs:float"},
                    {"paragraphSpacing", "xs:float"},
                    {"margin", "xmlLayout:vector4"},

                    {"firstVisibleCharacter", "xs:int"},
                    {"maxVisibleWords", "xs:int"},
                    {"colorGradient", "xmlLayout:colorblock"},
                    {"overrideColorTags", "xs:boolean"},

                    {"enableKerning", "xs:boolean"},
                    {"geometrySorting", "Normal,Reverse"},
                    {"enableCulling", "xs:boolean"},
                    {"richText", "xs:boolean"},
                    {"useMaxVisibleDescender", "xs:boolean"},
                    {"tintAllSprites", "xs:boolean"},
                    {"spriteAsset", "xs:string"},
                    {"parseCtrlCharacters", "xs:boolean"},
                    {"pageToDisplay", "xs:int"},

                    {"pixelsPerUnit", "xs:float"},
                    {"raycastTarget", "xs:boolean"},
                    {"isRightToLeftText", "xs:boolean" }

                };

                options.Add("alignment", String.Join(",", Enum.GetNames(typeof(TMPro.TextAlignmentOptions))));

                var mappingOptionsString = String.Join(",", Enum.GetNames(typeof(TMPro.TextureMappingOptions)));
                options.Add("horizontalMapping", mappingOptionsString);
                options.Add("verticalMapping", mappingOptionsString);

                options.Add("overflowMode", String.Join(",", Enum.GetNames(typeof(TMPro.TextOverflowModes))));

                return options;
            }
        }

        private static void RegisterCustomTypeHandlers()
        {
            ConversionExtensions.RegisterCustomTypeConverter(typeof(TMP_FontAsset),
             (value, xmlLayout) =>
             {
                 var font = XmlLayoutUtilities.LoadResource<TMP_FontAsset>(value);

                 if (font == null) Debug.LogWarning("[XmlLayout][TextMesh Pro] Unable to load TMP Font Asset '" + value + "'.");

                 return font;
             });

            ConversionExtensions.RegisterCustomTypeConverter(typeof(TMPro.FontStyles),
                (value) =>
                {
                    var stylesEntries = value.Split('|');
                    FontStyles styles = FontStyles.Normal;

                    foreach (var style in stylesEntries)
                    {
                        try
                        {
                            FontStyles s = (FontStyles)Enum.Parse(typeof(FontStyles), style);
                            styles |= s;
                        }
                        catch { }
                    }

                    return styles;
                });

            ConversionExtensions.RegisterCustomTypeConverter(typeof(TMPro.VertexGradient),
                (value, xmlLayout) =>
                {
                    var colorBlock = value.ToColorBlock(xmlLayout);

                    return new TMPro.VertexGradient(colorBlock.normalColor, colorBlock.highlightedColor, colorBlock.pressedColor, colorBlock.disabledColor);
                });

            ConversionExtensions.RegisterCustomTypeConverter(typeof(TMPro.TMP_SpriteAsset),
                (value) =>
                {
                    var spriteAsset = XmlLayoutUtilities.LoadResource<TMP_SpriteAsset>(value);

                    if (spriteAsset == null) Debug.LogWarning("[XmlLayout][TextMesh Pro] Unable to load TMP Sprite Asset '" + value + "'.");

                    return spriteAsset;
                });
        }
    }
}
#endif
