#if TEXTMESHPRO_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Xml;
using TMPro;
using System.Linq;

namespace UI.Xml.Tags
{
    public class TextMeshProMaterialTagHandler : ElementTagHandler
    {  
        public override MonoBehaviour primaryComponent
        {
            get { return null; }
        }

        public override string prefabPath
        {
            get { return null; }
        }             

        public override bool isCustomElement
        {
            get { return true; }
        }

        public override bool renderElement
        {
            get { return false; }
        }

        public override string elementGroup
        {
            get { return "defaultsOnly"; }
        }

        public override string elementChildType
        {
            get { return "none"; }
        }

        public override string extension
        {
            get { return "blank"; }
        }

        // Called by XmlLayout.HandleTextMeshProMaterialNode
        // (Which processes TextMeshProMaterial elements in <Defaults />)
        public static Material CreateMaterial(XmlLayout xmlLayout, AttributeDictionary materialAttributes)
        {
            if (!materialAttributes.ContainsKey("name"))
            {
                Debug.LogError("[XmlLayout][TextMeshProMaterial] Warning: no name defined.");
                return null;
            }

            // We need either a font or an existing material to base our new material on
            if (!materialAttributes.ContainsKey("font") && !materialAttributes.ContainsKey("baseMaterial"))
            {
                Debug.LogError("[XmlLayout][TextMeshProMaterial] Warning: no font or baseMaterial defined.");
                return null;
            }

            Material baseMaterial = null;

            if (materialAttributes.ContainsKey("font"))
            {
                var font = materialAttributes["font"].ChangeToType<TMP_FontAsset>();

                if (font == null) return null;                

                baseMaterial = font.material;
            }

            if (materialAttributes.ContainsKey("baseMaterial"))
            {
                if (xmlLayout.textMeshProMaterials.ContainsKey(materialAttributes["baseMaterial"]))
                {
                    baseMaterial = xmlLayout.textMeshProMaterials[materialAttributes["baseMaterial"]];
                }
                else
                {
                    baseMaterial = materialAttributes["baseMaterial"].ToMaterial();
                }
            }

            var material = new Material(baseMaterial);
            material.shaderKeywords = material.shaderKeywords;

            material.name = materialAttributes["name"];

            if (materialAttributes.ContainsKey("shader"))
            {
                material.shader = Shader.Find(materialAttributes["shader"]);
            }

            HandleShaderProperties(xmlLayout, material, materialAttributes);
            HandleShaderKeywords(material, materialAttributes);

            return material;
        }

        private static void HandleShaderProperties (XmlLayout xmlLayout, Material material, AttributeDictionary materialAttributes)
        {
            foreach (var attribute in materialAttributes)
            {
                if (attribute.Key.EndsWith("Color"))
                {
                    material.SetColor(GetShaderPropertyName(attribute.Key), attribute.Value.ToColor(xmlLayout));
                }
                else if (attribute.Key.EndsWith("Texture"))
                {
                    material.SetTexture(GetShaderPropertyName(attribute.Key), attribute.Value.ToTexture());
                }
                else if (attribute.Key.EndsWith("TextureOffset"))
                {
                    var textureName = GetShaderPropertyName(attribute.Key).Replace("Offset", "");
                    material.SetTextureOffset(textureName, attribute.Value.ToVector2());
                }
                else if (attribute.Key.EndsWith("Tiling"))
                {
                    var textureName = GetShaderPropertyName(attribute.Key).Replace("Tiling", "");
                    material.SetTextureScale(textureName, attribute.Value.ToVector2());
                }
                else if (attribute.Key.EndsWith("Rotation"))
                {
                    material.SetVector(GetShaderPropertyName(attribute.Key), attribute.Value.ToVector4());
                }
                else if (attribute.Key.EndsWith("Cubemap"))
                {
                    material.SetTexture(GetShaderPropertyName(attribute.Key), attribute.Value.ToCubeMap());
                }
                else if (attribute.Key.EndsWithAny(floatProperties) || attribute.Key.StartsWith("light"))
                {
                    material.SetFloat(GetShaderPropertyName(attribute.Key), attribute.Value.ToFloat());
                }
            }
        }

        private static void HandleShaderKeywords(Material material, AttributeDictionary materialAttributes)
        {
            if (materialAttributes.Any(a => a.Key.Contains("bevel")))
            {
                material.EnableKeyword("BEVEL_ON");
            }

            if (materialAttributes.Any(a => a.Key.StartsWith("underlay")))
            {
                var underlayType = materialAttributes.GetValue("underlayType");
                if (underlayType == "Inner")
                {
                    material.EnableKeyword("UNDERLAY_INNER");
                }
                else if (underlayType != "None")
                {
                    material.EnableKeyword("UNDERLAY_ON");
                }
            }

            if (materialAttributes.Any(a => a.Key.StartsWith("glow")))
            {
                material.EnableKeyword("GLOW_ON");
            }

            if (materialAttributes.ContainsKey("bevelType"))
            {
                var type = Enum.Parse(typeof(BevelType), materialAttributes["bevelType"]);
                material.SetFloat("_ShaderFlags", (int)type);
            }
        }

        private enum BevelType { OuterBevel, InnerBevel };

        private static string[] floatProperties = new string[]
        {
            "Softness",
            "Dilate",
            "Thickness",
            "SpeedX",
            "SpeedY",  
            "Width",
            "Amount",
            "Roundness",
            "Clamp",
            "Offset",  
            "OffsetX",
            "OffsetY",
            "Angle",
            "Power",                
        };

        private static Dictionary<string, string> _shaderPropertyNames = new Dictionary<string, string>();

        private static string GetShaderPropertyName(string attributeName)
        {
            if (_shaderPropertyNames.ContainsKey(attributeName)) return _shaderPropertyNames[attributeName];

            var name = attributeName;

            // Some of the TMP names don't match what you'd think
            // e.g. "Face -> Softness" is actually "Outline -> Softness"
            switch(name)
            {
                case "faceSoftness": name = "outlineSoftness"; break;
                case "outlineThickness": name = "outlineWidth"; break;
                case "bevelAmount": name = "bevel"; break;

                // and in a few cases, I've opted to use names I think are a bit clearer
                case "bumpMapTexture": name = "bumpMap"; break;
                case "bumpMapOutlineAmount": name = "bumpOutline"; break;
                case "bumpMapFaceAmount": name = "bumpFace"; break;

                case "envMapFaceColor": name = "reflectFaceColor"; break;
                case "envMapOutlineColor": name = "reflectOutlineColor"; break;
                case "envMapCubemap": name = "cube"; break;
                case "envMapMatrixRotation": name = "envMatrixRotation"; break;

                case "glowInnerAmount": name = "glowInner"; break;
                case "glowOuterAmount": name = "glowOuter"; break;                
            }

            name = name.Replace("Texture", "Tex");
            if (name != "lightAngle") name = name.Replace("light", "");
            name = name.Replace("Shadow", "");

            name = "_" + name[0].ToString().ToUpper() + name.Substring(1);

            // cache the result
            _shaderPropertyNames.Add(attributeName, name);

            return name;
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                var options = new Dictionary<string, string>()
                {
                    {"name", "xs:string"},
                    {"font", "xs:string"},
                    {"shader", "xs:string"},
                    {"baseMaterial", "xs:string"},

                    {"faceColor", "xmlLayout:color"},                    
                    {"faceSoftness", "xs:float"},
                    {"faceDilate", "xs:float"},
                    {"faceTexture", "xs:string"},
                    {"faceUVSpeedX", "xs:float"},
                    {"faceUVSpeedY", "xs:float"},
                    {"faceTextureOffset", "xmlLayout:vector2"},
                    {"faceTextureTiling", "xmlLayout:vector2"},

                    {"outlineColor", "xmlLayout:color" },
                    {"outlineTexture", "xs:string" },
                    {"outlineTextureOffset", "xmlLayout:vector2" },
                    {"outlineTextureTiling", "xmlLayout:vector2" },
                    {"outlineUVSpeedX", "xs:float" },
                    {"outlineUVSpeedY", "xs:float" },
                    {"outlineThickness", "xs:float" },
                    {"outlineSoftness", "xs:float" },
                    
                    {"bevelType", "InnerBevel,OuterBevel" },
                    {"bevelAmount", "xs:float"},
                    {"bevelWidth", "xs:float" },
                    {"bevelOffset", "xs:float"},
                    {"bevelClamp", "xs:float" },
                    {"bevelRoundness", "xs:float" },

                    {"lightAngle", "xs:float"},
                    {"lightSpecularColor", "xmlLayout:color" },
                    {"lightSpecularPower", "xs:float" },
                    {"lightReflectivity", "xs:float" },
                    {"lightDiffuseShadow", "xs:float" },
                    {"lightAmbientShadow", "xs:float" },
                    
                    {"bumpMapTexture", "xs:string" },
                    {"bumpMapOutlineAmount", "xs:float" },
                    {"bumpMapFaceAmount", "xs:float" },
                    
                    {"envMapFaceColor", "xmlLayout:color" },
                    {"envMapOutlineColor", "xmlLayout:color" },
                    {"envMapCubemap", "xs:string" },
                    {"envMapMatrixRotation", "xmlLayout:vector4" },    
                    
                    {"underlayType", "None,Normal,Inner" },
                    {"underlayColor", "xmlLayout:color"},
                    {"underlayOffsetX", "xs:float" },
                    {"underlayOffsetY", "xs:float" },
                    {"underlayDilate", "xs:float" },
                    {"underlaySoftness", "xs:float"},
                    
                    {"glowColor", "xmlLayout:color"},
                    {"glowOffset", "xs:float"},
                    {"glowInnerAmount", "xs:float"},
                    {"glowOuterAmount", "xs:float"},
                    {"glowPower", "xs:float"},    
                };
                
                return options;
            }
        }        
    }
}
#endif
