using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace UI.Xml
{
    public static class ConversionExtensions
    {
        static ConversionExtensions()
        {
            // if we're using a comma as a decimal separator, then don't allow it to be used as a regular number separator
            if (CultureInfo.NumberFormat.NumberDecimalSeparator == ",")
            {
                numberSeparators = new char[] { ' ', 'x' };
            }
        }

        private static CultureInfo _CultureInfo;
        public static CultureInfo CultureInfo
        {
            get
            {
                if (_CultureInfo == null)
                {
                    _CultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();

                    // This ensures that we use decimal points as a float separator regardless of what culture info is current
                    _CultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                }

                return _CultureInfo;
            }
        }

        private static Dictionary<Type, Func<string, XmlLayout, object>> CustomTypeConverters = new Dictionary<Type, Func<string, XmlLayout, object>>();

        public static void RegisterCustomTypeConverter(Type type, Func<string, object> convertMethod)
        {
            if (CustomTypeConverters.ContainsKey(type))
            {
                CustomTypeConverters[type] = (value, xmlLayout) => convertMethod(value);
            }
            else
            {
                CustomTypeConverters.Add(type, (value, xmlLayout) => convertMethod(value));
            }
        }

        public static void RegisterCustomTypeConverter(Type type, Func<string, XmlLayout, object> convertMethod)
        {
            if (CustomTypeConverters.ContainsKey(type))
            {
                CustomTypeConverters[type] = convertMethod;
            }
            else
            {
                CustomTypeConverters.Add(type, convertMethod);
            }
        }

        public static T ChangeToType<T>(this string str, XmlLayout xmlLayout = null)
        {
            return (T)str.ChangeToType(typeof(T), xmlLayout);
        }

        public static object ChangeToType(this string str, Type type, XmlLayout xmlLayout = null)
        {
            if (String.IsNullOrEmpty(str) || (str.ToLower() == "none" && type != typeof(string)) || (str.StartsWith("{") && str.EndsWith("}")))
            {
                return null;
            }

            if (CustomTypeConverters.ContainsKey(type))
            {
                return CustomTypeConverters[type].Invoke(str, xmlLayout);
            }

            // Special cases : Enums & specific types
            if (type.IsEnum)
            {
                return Enum.Parse(type, str, true);
            }

            switch (type.Name)
            {
                case "RectOffset":
                    return str.ToRectOffset();

                case "Rect":
                    return str.ToRect();

                case "Vector2":
                    return str.ToVector2();

                case "Vector3":
                    return str.ToVector3();

                case "Vector4":
                    return str.ToVector4();

                case "Boolean":
                case "bool":
                    return str.ToBoolean();

                case "Color":
                    return str.ToColor(xmlLayout);

                case "Color32":
                    return (Color32)str.ToColor(xmlLayout);

                case "ColorBlock":
                    return str.ToColorBlock(xmlLayout);

                case "Sprite":
                    return str.ToSprite();

                case "Texture":
                    return str.ToTexture();

                case "Quaternion":
                    return str.ToQuaternion();

                case "Font":
                    return str.ToFont();

                case "AudioClip":
                    return str.ToAudioClip();

                case "Material":
                    return str.ToMaterial();

                case "CursorInfo":
                    return str.ToCursorInfo();

                case "float":
                case "Single":
                    return str.ToFloat();

                case "int":
                case "Int32":
                case "Int64":
                    return Convert.ChangeType(str.ToInt(), type, CultureInfo);

                case "Transform":
                    return str.ToTransform();

#if DATEPICKER_PRESENT
                case "SerializableDate":
                    return new UI.Dates.SerializableDate(DateTime.Parse(str));
#endif
            }

            // Special handling for float lists
            if (typeof(IEnumerable<float>).IsAssignableFrom(type))
            {
                return GetFloatList(str);//.Select(i => (Single)i).ToList();
            }

            // Default behaviour
            return Convert.ChangeType(str, type, CultureInfo);
        }

        public static RectOffset ToRectOffset(this string str)
        {
            var rectArray = GetIntList(str);

            int left = 0, right = 0, top = 0, bottom = 0;

            left = rectArray[0];
            right = rectArray.Count > 1 ? rectArray[1] : left;
            top = rectArray.Count > 2 ? rectArray[2] : right;
            bottom = rectArray.Count > 3 ? rectArray[3] : top;

            return new RectOffset(left, right, top, bottom);
        }

        public static Rect ToRect(this string str)
        {
            var rectArray = GetFloatList(str);

            float x = 0, y = 0, width = 0, height = 0;

            x = rectArray[0];
            y = rectArray.Count > 1 ? rectArray[1] : x;
            width = rectArray.Count > 2 ? rectArray[2] : y;
            height = rectArray.Count > 3 ? rectArray[3] : width;

            return new Rect(x, y, width, height);
        }

        public static bool ToBoolean(this string str)
        {
            if (str == null) return false;

            return str.Equals("true", StringComparison.OrdinalIgnoreCase) || str.Equals("1");
        }

        public static Vector2 ToVector2(this string str)
        {
            var vectorArray = GetFloatList(str);

            float x = 0, y = 0;

            x = vectorArray[0];
            y = (vectorArray.Count > 1) ? vectorArray[1] : x;

            return new Vector2(x, y);
        }

        public static Vector3 ToVector3(this string str)
        {
            var vectorArray = GetFloatList(str);

            float x = 0, y = 0, z = 0;

            x = vectorArray[0];
            if (vectorArray.Count == 1)
            {
                y = z = x;
            }
            else
            {
                y = vectorArray.Count > 1 ? vectorArray[1] : x;

                if (vectorArray.Count > 2) z = vectorArray[2];
            }

            return new Vector3(x, y, z);
        }

        public static Vector4 ToVector4(this string str)
        {
            var vectorArray = GetFloatList(str);

            float x = 0, y = 0, z = 0, a = 0;

            x = vectorArray[0];

            if (vectorArray.Count == 1)
            {
                y = z = a = x;
            }
            else
            {
                y = vectorArray.Count > 1 ? vectorArray[1] : x;

                if (vectorArray.Count > 2) z = vectorArray[2];
                if (vectorArray.Count > 3) a = vectorArray[3];
            }

            return new Vector4(x, y, z, a);
        }

        private static Regex rgbTest = new Regex(@"(\d+[.]\d+)|(\d+)");
        public static Color ToColor(this string str, XmlLayout xmlLayout = null)
        {
            // MVVM color, will be set by properties
            if (str.StartsWith("{")) return Color.white;

            str = str.ToLower();

            MatchCollection matches;

            // new: by user-defined name
            if (xmlLayout != null)
            {
                if (xmlLayout.namedColors.ContainsKey(str))
                {
                    return xmlLayout.namedColors[str];
                }
            }

            // a) HTML code
            if (str.StartsWith("#"))
            {
                return HexStringToColor(str);
            }

            // b) RGB / RGBA
            if (str.StartsWith("rgb"))
            {
                matches = rgbTest.Matches(str);

                if (matches.Count >= 3)
                {

                    float r = GetColorValue(matches[0].Value);
                    float g = GetColorValue(matches[1].Value);
                    float b = GetColorValue(matches[2].Value);

                    float a = 1f;
                    if (matches.Count == 4)
                    {
                        a = GetColorValue(matches[3].Value);
                    }

                    return new Color(r, g, b, a);
                }
                else
                {
                    Debug.LogWarning("[XmlLayout] Warning: '" + str + "' is not a valid Color value.");
                }
            }

            // c) By name
            var propertyInfo = typeof(Color).GetProperty(str);
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(Color))
            {
                return (Color)propertyInfo.GetValue(null, XmlLayoutUtilities.BindingFlags, null, null, null);
            }

            Debug.LogWarning("[XmlLayout] Warning: '" + str + "' is not a valid Color value.");

            // default
            return Color.clear;
        }

        private static float GetColorValue(string match)
        {
            return float.Parse(match, CultureInfo);
        }

        public static Color HexStringToColor(string hex)
        {
            hex = hex.Replace ("0x", string.Empty);
            hex = hex.Replace ("#", string.Empty);

            if (hex.Length < 6) return Color.clear;

            byte a = 255;
            byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);

            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber, CultureInfo);
            }

             return ((Color)new Color32(r,g,b,a));
        }

        private static string[] GetParts(string str, char[] separators, bool trim)
        {
            return (trim ? str.Trim('(', ')') : str).Split(separators);
        }

        private static char[] numberSeparators = new char[] { ' ', 'x', ',' };

        public static List<int> GetIntList(string str)
        {
            List<int> list = new List<int>();
            if (string.IsNullOrEmpty(str)) return list;

            var parts = GetParts(str, numberSeparators, true);

            foreach(var part in parts)
            {
                int result = 0;
                if(!string.IsNullOrEmpty(part)) int.TryParse(part, NumberStyles.Any, CultureInfo, out result);
                list.Add(result);
            }

            return list;

            /*return parts
                      .Select(s =>
                        {
                            int result = 0;
                            int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo, out result);
                            return result;
                        })
                      .ToList();*/
        }

        public static List<float> GetFloatList(string str)
        {
            List<float> list = new List<float>();
            if (string.IsNullOrEmpty(str)) return list;

            var parts = GetParts(str, numberSeparators, true);

            foreach(var part in parts)
            {
                float result = 0;
                if (!string.IsNullOrEmpty(part)) float.TryParse(part, NumberStyles.Float, CultureInfo, out result);
                list.Add(result);
            }

            return list;

            /*return str.Trim('(', ')')
                      .Split(' ', 'x', ',')
                      .Select(s =>
                      {
                          float result = 0;
                          float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
                          return result;
                      })
                      .ToList();*/
        }

        private static char[] colorSeparators = new char[] { ' ', '|' };

        public static List<Color> GetColorList(string str, XmlLayout xmlLayout = null)
        {
            List<Color> list = new List<Color>();
            if (string.IsNullOrEmpty(str)) return list;

            var parts = GetParts(str, colorSeparators, false);

            foreach(var part in parts)
            {
                list.Add(part.ToColor(xmlLayout));
            }

            return list;

            /*return str.Split(' ', '|')
                      .Select(s => s.ToColor(xmlLayout))
                      .ToList();*/
        }

        public static ColorBlock ToColorBlock(this string str, XmlLayout xmlLayout = null)
        {
            var colorBlock = new ColorBlock();
            colorBlock.normalColor = colorBlock.disabledColor = colorBlock.pressedColor = Color.white;
            colorBlock.disabledColor = new Color(1, 1, 1, 0.5f);
            colorBlock.colorMultiplier = 1;

            var colorList = GetColorList(str, xmlLayout);

            if (colorList.Count > 0) colorBlock.normalColor = colorList[0];
            if (colorList.Count > 1) colorBlock.highlightedColor = colorList[1];
#if UNITY_2019_1_OR_NEWER
            if (colorList.Count > 2) colorBlock.pressedColor = colorBlock.selectedColor = colorList[2];
#else
            if (colorList.Count > 2) colorBlock.pressedColor = colorList[2];
#endif
            if (colorList.Count > 3) colorBlock.disabledColor = colorList[3];

            return colorBlock;
        }

        public static Sprite ToSprite(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.ToLower() == "none")
            {
                return null;
            }

            var sprite = XmlLayoutUtilities.LoadResource<Sprite>(str);

            if (sprite == null)
            {
                Debug.LogError("[XmlLayout] Unable to load sprite '" + str + "'. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
            }

            return sprite;
        }

        public static Texture ToTexture(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.ToLower() == "none")
            {
                return null;
            }

            var texture = XmlLayoutUtilities.LoadResource<Texture>(str);

            if (texture == null)
            {
                var sprite = XmlLayoutUtilities.LoadResource<Sprite>(str);
                if (sprite == null)
                {
                    Debug.LogError("[XmlLayout] Unable to load texture '" + str + "'. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
                }
                else
                {
                    texture = sprite.texture as Texture;
                }
            }

            return texture;
        }

        public static Texture2D ToTexture2D(this string str)
        {
            return (Texture2D)str.ToTexture();
        }

        public static Cubemap ToCubeMap(this string str)
        {
            return (Cubemap)str.ToTexture();
        }

        public static Quaternion ToQuaternion(this string str)
        {
            var floatArray = GetFloatList(str);

            Quaternion result = new Quaternion();

            if (floatArray.Count >= 4)
            {
                return new Quaternion(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
            }
            else
            {
                var x = floatArray[0];
                var y = floatArray.Count > 1 ? floatArray[1] : 0;
                var z = floatArray.Count > 2 ? floatArray[2] : 0;

                result.eulerAngles = new Vector3(x, y, z);
            }

            return result;
        }

        public static Font ToFont(this string str)
        {
            var font = XmlLayoutUtilities.LoadResource<Font>("Fonts/" + str);
            if (font == null) font = XmlLayoutUtilities.LoadResource<Font>(str);

            if (font == null)
            {
                Debug.LogWarning("Font '" + str + "' not found. Please ensure that it is located within a Resources folder or XmlLayout Resource Database. (Reverting to Arial)");

                return Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            }

            return font;
        }

        public static RuntimeAnimatorController ToRuntimeAnimatorController(this string str)
        {
            if (str.ToLower() == "none")
            {
                return null;
            }

            var animationController = XmlLayoutUtilities.LoadResource<RuntimeAnimatorController>(str);

            if (animationController == null)
            {
                Debug.Log("Animation Controller '" + str + "' not found. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
            }

            return animationController;
        }

        public static AudioClip ToAudioClip(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.ToLower() == "none")
            {
                return null;
            }

            var audioClip = XmlLayoutUtilities.LoadResource<AudioClip>(str);

            if (audioClip == null)
            {
                Debug.Log("Audio Clip '" + str + "' not found. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
            }

            return audioClip;
        }

        public static Material ToMaterial(this string str)
        {
            if (str.ToLower() == "none")
            {
                return null;
            }

            var material = XmlLayoutUtilities.LoadResource<Material>(str);

            if (material == null)
            {
                Debug.Log("Material '" + str + "' not found. Please ensure that it is located within a Resources folder or XmlLayout Resource Database.");
            }

            return material;
        }

        public static XmlLayoutCursorController.CursorInfo ToCursorInfo(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.ToLower() == "none")
            {
                return null;
            }

            var strings = str.Split('|');

            var cursor = strings[0].ToTexture2D();
            var hotspot = Vector2.zero;

            if (strings.Length > 1)
            {
                hotspot = strings[1].ToVector2();
            }

            return new XmlLayoutCursorController.CursorInfo() { cursor = cursor, hotspot = hotspot };
        }

        public static float ToFloat(this string str)
        {
            float f = 0;

            float.TryParse(str, NumberStyles.Float, CultureInfo, out f);

            return f;
        }

        public static int ToInt(this string str)
        {
            int i = 0;

            int.TryParse(str, NumberStyles.Integer, CultureInfo, out i);

            return i;
        }

        public static Transform ToTransform(this string str)
        {
            return XmlLayoutUtilities.LoadResource<Transform>(str);
        }

        public static List<string> ToClassList(this string str)
        {
            return str != null
                ? str.Split(',', ' ').Select(s => s.Trim().ToLower()).ToList()
                : new List<string>();
        }

        public static string ToString(object o)
        {
            if (o == null) return "";

            var type = o.GetType();

            if (type.Name.StartsWith("Vector"))
            {
                return o.ToString().Replace(" ", "");
            }

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                var obj = o as UnityEngine.Object;
                // try to locate the asset in the resource database
                var path = XmlLayoutResourceDatabase.instance.GetResourcePath(obj);

                if (path == null)
                {
                    path = "DynamicResource_" + Guid.NewGuid().ToString() + "_" + obj.name;
                    XmlLayoutResourceDatabase.instance.AddResource(path, obj);
                }

                return path;
            }

            return o.ToString();
        }

        /// <summary>
        /// Modified from: https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        /// </summary>
        public static bool IsNumericType(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
