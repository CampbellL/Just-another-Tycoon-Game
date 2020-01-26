using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Xml;
using UI.Xml.CustomAttributes;
using UI.Xml.Tags;
using UI.Xml.Configuration;

namespace UI.Xml
{
    public static class XmlLayoutUtilities
    {
        private static List<Type> m_TagHandlerTypes = null;
        private static List<string> m_TagHandlerNames = null;
        private static Dictionary<string, ElementTagHandler> m_TagHandlers = new Dictionary<string, ElementTagHandler>(StringComparer.OrdinalIgnoreCase);

        private static List<Type> m_CustomXmlAttributeTypes = null;
        private static Dictionary<string, CustomXmlAttribute> m_CustomXmlAttributes = new Dictionary<string, CustomXmlAttribute>(StringComparer.OrdinalIgnoreCase);
        private static List<string> m_CustomAttributeTypeNames = null;

        public static XmlLayoutConfiguration XmlLayoutConfiguration
        {
            get
            {
                if (m_XmlLayoutConfiguration == null) m_XmlLayoutConfiguration = Resources.Load<XmlLayoutConfiguration>("XmlLayout_Configuration");
                return m_XmlLayoutConfiguration;
            }
        }
        private static XmlLayoutConfiguration m_XmlLayoutConfiguration;

        private static Dictionary<string, UnityEngine.Object> m_CachedResources = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);

        private static List<Type> m_XmlLayoutControllerTypes = null;
        private static List<string> m_XmlLayoutControllerNames = null;

        public static void Reset()
        {
            XmlLayoutAssemblyHelper.Reset();

            m_TagHandlerTypes = null;
            m_TagHandlerNames = null;

            m_CustomXmlAttributeTypes = null;
            m_CustomXmlAttributes.Clear();
            m_CustomAttributeTypeNames = null;

            m_XmlLayoutControllerTypes = null;
            m_XmlLayoutControllerNames = null;
        }


        private static void LoadTagHandlerTypesIfNecessary()
        {
            if (m_TagHandlerTypes == null || m_TagHandlerTypes.Count == 0)
            {
                m_TagHandlerTypes = new List<Type>();
                var assemblies = XmlLayoutAssemblyHelper.GetAssemblyNames();
                var elementTagHandlerType = typeof(ElementTagHandler);

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        m_TagHandlerTypes.AddRange(Assembly.Load(assembly)
                                                           .GetTypes()
                                                           .Where(t => !t.IsAbstract && t.IsSubclassOf(elementTagHandlerType))
                                                           .ToList());
                    }
                    catch
                    {
                        // if Assembly.Load fails, then ignore that assembly
                    }
                }

                m_TagHandlerNames = m_TagHandlerTypes.Select(t => GetTagName(t)).ToList();
            }
        }

        private static Dictionary<Type, string> cachedTagNames = new Dictionary<Type, string>();

        internal static string GetTagName(Type tagHandler)
        {
            if (cachedTagNames.ContainsKey(tagHandler))
            {
                return cachedTagNames[tagHandler];
            }

            var attribute = (ElementTagHandlerAttribute)Attribute.GetCustomAttribute(tagHandler, typeof(ElementTagHandlerAttribute));

            string tagName = null;

            if (attribute != null && !String.IsNullOrEmpty(attribute.TagName))
            {
                tagName = attribute.TagName;
            }
            else
            {
                tagName = tagHandler.Name.Replace("TagHandler", string.Empty);
            }

            cachedTagNames.Add(tagHandler, tagName);

            return tagName;
        }

        public static List<Type> GetXmlLayoutControllerTypes()
        {
            if (m_XmlLayoutControllerTypes == null || m_XmlLayoutControllerTypes.Count == 0)
            {
                var assemblies = XmlLayoutAssemblyHelper.GetAssemblyNames();
                m_XmlLayoutControllerNames = new List<string>();
                m_XmlLayoutControllerTypes = new List<Type>();
                var xmlLayoutControllerType = typeof(XmlLayoutController);

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        m_XmlLayoutControllerTypes.AddRange(Assembly.Load(assembly)
                                                                    .GetTypes()
                                                                    .Where(t => !t.IsAbstract && t.IsSubclassOf(xmlLayoutControllerType))
                                                                    .ToList());
                    }
                    catch
                    {
                        // if Assembly.Load fails, then ignore that assembly
                    }
                }

                m_XmlLayoutControllerNames = m_XmlLayoutControllerTypes.Select(t => t.Name).ToList();
            }

            return m_XmlLayoutControllerTypes;
        }

        public static List<string> GetXmlLayoutControllerNames()
        {
            if (m_XmlLayoutControllerNames == null || m_XmlLayoutControllerNames.Count == 0) GetXmlLayoutControllerTypes();

            return m_XmlLayoutControllerNames;
        }

        public static Type GetXmlLayoutControllerType(string controllerName)
        {
            return GetXmlLayoutControllerTypes().FirstOrDefault(c => c.Name == controllerName);
        }

        public static ElementTagHandler GetXmlTagHandler(string tag)
        {
            if (!m_TagHandlers.ContainsKey(tag))
            {
                LoadTagHandlerTypesIfNecessary();

                var type = m_TagHandlerTypes.FirstOrDefault(t => GetTagName(t).Equals(tag, StringComparison.OrdinalIgnoreCase));

                if (type == null)
                {
                    bool error = true;

                    // if this happens, we've failed to load tag handlers for some reason
                    if (tag == "XmlLayout")
                    {
                        Reset();
                        XmlLayoutAssemblyHelper.ClearAssemblyNames();

                        LoadTagHandlerTypesIfNecessary();

                        type = m_TagHandlerTypes.FirstOrDefault(t => GetTagName(t).Equals(tag, StringComparison.OrdinalIgnoreCase));

                        if (type != null) error = false;
                    }

                    if (error)
                    {
                        Debug.LogError("[XmlLayout] Unknown tag '" + tag + "'.\r\nTag Handlers must inherit from 'ElementTagHandler', and must be named {Tag}TagHandler.");
                        return null;
                    }
                }

                m_TagHandlers.Add(tag, (ElementTagHandler)Activator.CreateInstance(type));
            }

            return m_TagHandlers[tag];
        }

        public static List<ElementTagHandler> GetXmlTagHandlers()
        {
            LoadTagHandlerTypesIfNecessary();

            return m_TagHandlers.Select(t => t.Value).ToList();
        }

        public static List<string> GetXmlTagHandlerNames()
        {
            LoadTagHandlerTypesIfNecessary();

            return m_TagHandlerNames;
        }

        private static void PopulateCustomAttributeDataIfNecessary()
        {
            if (m_CustomXmlAttributeTypes == null || m_CustomXmlAttributeTypes.Count == 0)
            {
                m_CustomXmlAttributeTypes = new List<Type>();

                var assemblies = XmlLayoutAssemblyHelper.GetAssemblyNames();

                var customXmlAttributeType = typeof(CustomXmlAttribute);

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        m_CustomXmlAttributeTypes.AddRange(Assembly.Load(assembly)
                                                            .GetTypes()
                                                            .Where(t => !t.IsAbstract && t.IsSubclassOf(customXmlAttributeType))
                                                            .ToList());
                    }
                    catch
                    {
                        // if Assembly.Load fails, then ignore that assembly
                    }
                }

                m_CustomAttributeTypeNames = m_CustomXmlAttributeTypes.Select(s => s.Name.Replace("Attribute", string.Empty)).ToList();
            }
        }

        public static List<string> GetCustomAttributeNames()
        {
            PopulateCustomAttributeDataIfNecessary();

            return m_CustomAttributeTypeNames;
        }

        public static Dictionary<CustomXmlAttribute.eAttributeGroup, List<string>> GetGroupedCustomAttributeNames()
        {
            PopulateCustomAttributeDataIfNecessary();

            var dictionary = new Dictionary<CustomXmlAttribute.eAttributeGroup, List<string>>();

            foreach (var customXmlAttribute in m_CustomAttributeTypeNames)
            {
                var instance = GetCustomAttribute(customXmlAttribute);

                if (!dictionary.ContainsKey(instance.AttributeGroup)) dictionary.Add(instance.AttributeGroup, new List<string>());

                dictionary[instance.AttributeGroup].Add(customXmlAttribute);
            }

            return dictionary;
        }

        private static List<Type> GetCustomAttributeTypes()
        {
            PopulateCustomAttributeDataIfNecessary();

            return m_CustomXmlAttributeTypes;
        }

        private static Dictionary<string, bool> isCustomAttributeCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public static bool IsCustomAttribute(string attributeName)
        {
            if (isCustomAttributeCache.ContainsKey(attributeName)) return isCustomAttributeCache[attributeName];

            bool result = GetCustomAttributeNames().Contains(attributeName, StringComparer.OrdinalIgnoreCase);

            isCustomAttributeCache.Add(attributeName, result);

            return result;
        }

        public static CustomXmlAttribute GetCustomAttribute(string attributeName)
        {
            PopulateCustomAttributeDataIfNecessary();

            if (!m_CustomXmlAttributes.ContainsKey(attributeName))
            {
                var attributeTypeName = attributeName + "Attribute";

                var type = m_CustomXmlAttributeTypes.FirstOrDefault(t => t.Name.Equals(attributeTypeName, StringComparison.OrdinalIgnoreCase));

                if (type == null)
                {
                    Debug.LogWarning("[XmlLayout] Unknown Custom Attribute '" + attributeName + "'.");
                    return null;
                }

                m_CustomXmlAttributes.Add(attributeName, (CustomXmlAttribute)Activator.CreateInstance(type));
            }

            return m_CustomXmlAttributes[attributeName];
        }

        public static AttributeDictionary MergeAttributes(AttributeDictionary defaults, AttributeDictionary elementAttributes)
        {
            var result = defaults.Clone();

            if (elementAttributes == null) return result;

            foreach (var attribute in elementAttributes)
            {
                if (!result.ContainsKey(attribute.Key))
                {
                    result.Add(attribute.Key, attribute.Value);
                }
                else
                {
                    result[attribute.Key] = attribute.Value;
                }
            }

            return result;
        }

        public static T LoadResource<T>(string path, bool ignoreCache = false)
            where T : UnityEngine.Object
        {
            if (path == null) return null;

            UnityEngine.Object resource = null;

            //var t = DateTime.UtcNow;
            // New resource database code
            resource = XmlLayoutResourceDatabase.instance.GetResource<T>(path);
            //Debug.Log("Load Resource (" + path + ") => " + (DateTime.UtcNow - t).TotalMilliseconds + "ms");

            if (resource != null) return resource as T;

            // Otherwise, fall back to original behaviour

            if (!ignoreCache)
            {
                // include the type name in the cache path, so that if we try to cache two different objects that share the same path, we can still access them both
                var cachePath = String.Format("{0}|{1}", typeof(T).Name, path);

                if (!m_CachedResources.TryGetValue(cachePath, out resource))
                {
                    if (path.Contains(":"))
                    {
                        var resources = Resources.LoadAll<T>(path.Substring(0, path.IndexOf(":")));
                        var subName = path.Substring(path.IndexOf(":") + 1);

                        foreach (var r in resources)
                        {
                            if (subName == r.name)
                            {
                                resource = r;
                                break;
                            }
                        }
                    }
                    else
                    {
                        resource = Resources.Load<T>(path);
                    }

                    if (resource != null)
                    {
                        m_CachedResources.Add(cachePath, resource);
                    }
                }
            }
            else
            {
                resource = Resources.Load<T>(path);
            }

            return resource as T;
        }

        internal static System.Reflection.BindingFlags BindingFlags
            = System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.IgnoreCase
            | System.Reflection.BindingFlags.Instance;

    }
}
