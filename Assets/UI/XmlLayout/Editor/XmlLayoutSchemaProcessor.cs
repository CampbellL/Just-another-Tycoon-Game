using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

using System.IO;
using System.Text;
using System.Xml.Serialization;

using System.Reflection;

namespace UI.Xml
{
    public class XmlLayoutSchemaProcessor
    {
        readonly string baseXSDPath = string.Empty;
        XElement schemaElement;
        readonly XNamespace ns;

        // by group
        Dictionary<CustomXmlAttribute.eAttributeGroup, Dictionary<string, CustomXmlAttribute>> customAttributesToAddByGroup = new Dictionary<CustomXmlAttribute.eAttributeGroup, Dictionary<string, CustomXmlAttribute>>();
        Dictionary<string, Dictionary<string, CustomXmlAttribute>> customAttributesToAddByElement = new Dictionary<string, Dictionary<string, CustomXmlAttribute>>();
        Dictionary<string, Dictionary<string, ElementTagHandler>> customElementTagHandlersToAdd = new Dictionary<string, Dictionary<string, ElementTagHandler>>();

        List<CustomXmlAttributeGroup> customAttributeGroupsToAdd = new List<CustomXmlAttributeGroup>();

        Dictionary<string, string> enumPatterns = new Dictionary<string, string>();

        Dictionary<string, XmlSchemaElement> customElementsAdded = new Dictionary<string, XmlSchemaElement>();

        /// <summary>
        /// Entry point for this process
        /// Called whenever Unity finishes loading scripts and compiling them
        /// </summary>
        [DidReloadScripts(1)]
        public static void ProcessXmlSchema()
        {
            // don't do this at runtime
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // This is sometimes called too early, before XmlLayout has loaded it's configuration file, and should then be ignored.
            if (XmlLayoutUtilities.XmlLayoutConfiguration == null) return;

            // If we're not automatically doing this, skip it
            if (!XmlLayoutUtilities.XmlLayoutConfiguration.AutomaticallyUpdateXSDFile) return;

            ProcessXmlSchema(false);
        }

        public static void ProcessXmlSchema(bool force)
        {
            var schemaAssetPath = string.Format("{0}/{1}", Application.dataPath, AssetDatabase.GetAssetPath(XmlLayoutUtilities.XmlLayoutConfiguration.BaseXSDFile).Substring("Assets/".Length));
            var processor = new XmlLayoutSchemaProcessor(schemaAssetPath);

            processor.ProcessCustomAttributes();
            processor.ProcessCustomAttributeGroups();
            processor.ProcessCustomTags();

            if (force || processor.HasChanges())
            {
                processor.Output(string.Format("{0}/{1}", Application.dataPath, AssetDatabase.GetAssetPath(XmlLayoutUtilities.XmlLayoutConfiguration.XSDFile).Substring("Assets/".Length)));
            }

            processor = null;
        }

        public XmlLayoutSchemaProcessor(string schemaPath)
        {
            baseXSDPath = schemaPath;

            XDocument xDoc = XDocument.Load(schemaPath);
            ns = XNamespace.Get(@"http://www.w3.org/2001/XMLSchema");

            schemaElement = xDoc.Elements().First();
        }

        public bool HasChanges()
        {
            return customAttributesToAddByElement.Count > 0 || customAttributesToAddByGroup.Count > 0 || customElementTagHandlersToAdd.Count > 0;
        }

        public void Output(string outputPath)
        {
            if (!XmlLayoutUtilities.XmlLayoutConfiguration.SuppressXSDUpdateMessage) Debug.Log("[XmlLayout] Updating XSD File.");

            var xmlTextReader = new XmlTextReader(baseXSDPath);

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += ValidationCallback;
            schemaSet.Add(XmlSchema.Read(xmlTextReader, ValidationCallback));
            schemaSet.Compile();

            XmlSchema schema = null;
            foreach (XmlSchema s in schemaSet.Schemas())
            {
                schema = s;
            }

            schema.Namespaces.Add(String.Empty, String.Empty);
            schema.Namespaces.Add("xmlLayout", @"XmlLayout");

            if (customAttributesToAddByGroup.Count > 0)
            {
                foreach (var group in customAttributesToAddByGroup)
                {
                    // Custom attribute groups are handled differently
                    if (group.Key == CustomXmlAttribute.eAttributeGroup.Custom) continue;

                    XmlSchemaAttributeGroup schemaAttributeGroup = null;
                    if (group.Key != CustomXmlAttribute.eAttributeGroup.Custom)
                    {
                        foreach (DictionaryEntry attributeGroup in schema.AttributeGroups)
                        {
                            if (attributeGroup.Key.ToString().EndsWith(GetAttributeGroupName(group.Key), StringComparison.OrdinalIgnoreCase))
                            {
                                schemaAttributeGroup = (XmlSchemaAttributeGroup)attributeGroup.Value;
                            }
                        }
                    }

                    foreach (var attribute in group.Value)
                    {
                        if (attribute.Value.ValueDataType.Contains(','))
                        {
                            CreateEnumSimpleType(attribute.Value.ValueDataType, schema);

                            schemaAttributeGroup.Attributes.Add(new XmlSchemaAttribute { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(enumPatterns[attribute.Value.ValueDataType], @"XmlLayout") });
                        }
                        else
                        {
                            schemaAttributeGroup.Attributes.Add(new XmlSchemaAttribute() { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(attribute.Value.ValueDataType) });
                        }
                    }
                }
            }

            if (customAttributeGroupsToAdd.Count > 0)
            {
                foreach (var group in customAttributeGroupsToAdd)
                {
                    XmlSchemaAttributeGroup schemaAttributeGroup = new XmlSchemaAttributeGroup() { Name = group.GroupName };

                    foreach (var attributeType in group.CustomXmlAttributes)
                    {
                        var tagHandler = GetTestInstance<CustomXmlAttribute>(attributeType);

                        var name = tagHandler.GetType().Name;
                        name = char.ToLower(name[0]) + name.Substring(1).Replace("Attribute", string.Empty);

                        schemaAttributeGroup.Attributes.Add(new XmlSchemaAttribute() { Name = name, SchemaTypeName = new XmlQualifiedName(tagHandler.ValueDataType)});
                    }

                    schema.Items.Add(schemaAttributeGroup);
                }
            }

            if (customElementTagHandlersToAdd.Count > 0)
            {
                var defaultsElement = (XmlSchemaElement)schema.Elements[new XmlQualifiedName("Defaults", @"XmlLayout")];
                var defaultsComplexType = (XmlSchemaComplexType)defaultsElement.SchemaType;
                var defaultsComplexContent = (XmlSchemaComplexContentExtension)defaultsComplexType.ContentModel.Content;
                var defaultsChoice = (XmlSchemaChoice)defaultsComplexContent.Particle;

                foreach(var group in customElementTagHandlersToAdd)
                {
                    XmlSchemaGroup schemaElementGroup = null;
                    foreach (DictionaryEntry elementGroup in schema.Groups)
                    {
                        if (elementGroup.Key.ToString().EndsWith(group.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            schemaElementGroup = (XmlSchemaGroup)elementGroup.Value;
                        }
                    }

                    // We need to create the group
                    if (schemaElementGroup == null)
                    {
                        schemaElementGroup = new XmlSchemaGroup() { Name = group.Key };
                        schemaElementGroup.Particle = new XmlSchemaChoice() { };
                        schema.Items.Add(schemaElementGroup);
                    }

                    foreach (var tag in group.Value)
                    {
                        var useChoice = (!String.IsNullOrEmpty(tag.Value.elementChildType) && tag.Value.elementChildType != "none");

                        var element = new XmlSchemaElement() { Name = tag.Key };
                        schema.Items.Add(element);

                        var complexType = new XmlSchemaComplexType();
                        var complexContent = new XmlSchemaComplexContent() { IsMixed = true };
                        var choice = new XmlSchemaChoice() { MinOccurs = 0, MaxOccursString = "unbounded" };
                        var extension = new XmlSchemaComplexContentExtension() { Particle = useChoice ? choice : null, BaseTypeName = new XmlQualifiedName(tag.Value.extension, @"XmlLayout") };

                        element.SchemaType = complexType;
                        complexType.ContentModel = complexContent;
                        complexContent.Content = extension;

                        if (useChoice)
                        {
                            choice.Items.Add(new XmlSchemaGroupRef { RefName = new XmlQualifiedName(tag.Value.elementChildType, @"XmlLayout") });
                        }

                        // add attributes
                        var attributes = tag.Value.attributes;
                        if (attributes.Count > 0)
                        {
                            foreach (var attribute in attributes)
                            {
                                // If the value contains commas, then it is a comma-seperated list and should be converted into an enum
                                if (attribute.Value.Contains(','))
                                {
                                    if (!enumPatterns.ContainsKey(attribute.Value))
                                    {
                                        CreateEnumSimpleType(attribute.Value, schema);
                                    }

                                    extension.Attributes.Add(new XmlSchemaAttribute { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(enumPatterns[attribute.Value], @"XmlLayout") });
                                }
                                else
                                {
                                    extension.Attributes.Add(new XmlSchemaAttribute() { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(attribute.Value) });
                                }
                            }
                        }

                        var attributeGroups = tag.Value.attributeGroups;
                        if (attributeGroups.Count > 0)
                        {
                            foreach (var attributeGroup in attributeGroups)
                            {
                                extension.Attributes.Add(new XmlSchemaAttributeGroupRef() { RefName = new XmlQualifiedName(attributeGroup, @"XmlLayout") });
                            }
                        }

                        var customAttributeGroups = tag.Value.customAttributeGroups;
                        if (customAttributeGroups.Count > 0)
                        {
                            foreach (var customAttributeGroupType in customAttributeGroups)
                            {
                                var customAttributeGroup = GetTestInstance<CustomXmlAttributeGroup>(customAttributeGroupType);
                                extension.Attributes.Add(new XmlSchemaAttributeGroupRef() { RefName = new XmlQualifiedName(customAttributeGroup.GroupName, @"XmlLayout") });
                            }
                        }

                        // add the reference to the default group
                        var refElement = new XmlSchemaElement() { RefName = new XmlQualifiedName(tag.Key, @"XmlLayout") };
                        schemaElementGroup.Particle.Items.Add(refElement);

                        if (!group.Key.Equals("default", StringComparison.OrdinalIgnoreCase))
                        {
                            var item = new XmlSchemaElement { RefName = new XmlQualifiedName(tag.Key, @"XmlLayout") };
                            defaultsChoice.Items.Add(item);
                        }

                        if (tag.Value.allowAnyAttribute)
                        {
                            extension.AnyAttribute = new XmlSchemaAnyAttribute() { ProcessContents = XmlSchemaContentProcessing.Lax };
                        }

                        customElementsAdded.Add(tag.Key, element);
                    }
                }
            }

#if TEXTMESHPRO_PRESENT
            AddElementToParent(schema, "Button", "TextMeshPro");
            AddElementToParent(schema, "Toggle", "TextMeshPro");
#endif

            if (customAttributesToAddByElement.Count > 0)
            {
                foreach(var elementKVP in customAttributesToAddByElement)
                {
                    try
                    {
                        var attributes = elementKVP.Value;

                        var element = (XmlSchemaElement)schema.Elements[new XmlQualifiedName(elementKVP.Key, @"XmlLayout")];

                        if (element == null)
                        {
                            element = customElementsAdded[elementKVP.Key];
                        }

                        var elementComplexType = (XmlSchemaComplexType)element.SchemaType;
                        var elementComplexContent = (XmlSchemaComplexContentExtension)elementComplexType.ContentModel.Content;

                        foreach (var attribute in attributes)
                        {
                            // If the value contains commas, then it is a comma-seperated list and should be converted into an enum
                            if (attribute.Value.ValueDataType.Contains(','))
                            {
                                if (!enumPatterns.ContainsKey(attribute.Value.ValueDataType))
                                {
                                    CreateEnumSimpleType(attribute.Value.ValueDataType, schema);
                                }

                                elementComplexContent.Attributes.Add(new XmlSchemaAttribute { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(enumPatterns[attribute.Value.ValueDataType], @"XmlLayout") });
                            }
                            else
                            {
                                elementComplexContent.Attributes.Add(new XmlSchemaAttribute() { Name = attribute.Key, SchemaTypeName = new XmlQualifiedName(attribute.Value.ValueDataType) });
                            }
                        }
                    }
                    catch { }
                }
            }

            if (XmlLayoutUtilities.XmlLayoutConfiguration.AllowAnyAttribute)
            {
                var simpleAttributes = (XmlSchemaAttributeGroup)schema.AttributeGroups[new XmlQualifiedName("simpleAttributes", @"XmlLayout")];
                simpleAttributes.AnyAttribute = new XmlSchemaAnyAttribute() { ProcessContents = XmlSchemaContentProcessing.Lax };
            }

            // output to file
            using (FileStream file = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (XmlTextWriter xwriter = new XmlTextWriter(file, new UTF8Encoding()))
                {
                    xwriter.Formatting = Formatting.Indented;
                    schema.Write(xwriter);
                }
            }

            // release the lock on the file
            xmlTextReader.Close();

            SubstituteMVVMAttributeDataTypes(outputPath);
        }

        private Dictionary<Type, object> testInstances = new Dictionary<Type, object>();
        private T GetTestInstance<T>(Type type)
        {
            if (testInstances.ContainsKey(type)) return (T)testInstances[type];

            var t = (T)Activator.CreateInstance(type);
            testInstances.Add(type, t);

            return t;
        }

        public static List<string> MVVMDataTypeSubstitutes = new List<string>()
        {
            "boolean",
            "float",
            "integer",
            "int",
            //"token"
        };

        void SubstituteMVVMAttributeDataTypes(string outputPath)
        {
            // Now use string replacement to replace default data types with XmlLayout MVVM ones
            var rawXsd = File.ReadAllText(outputPath);

            foreach (var type in MVVMDataTypeSubstitutes)
            {
                var original = String.Format("\"xs:{0}\"", type);
                var substitute = String.Format("\"xmlLayout:{0}\"", type);

                // the last entries no longer match the pattern (due to the lack of the post quotation mark, so there's no need to skip the last matches)
                //rawXsd = new StringBuilder(rawXsd).Replace(original, substitute, 0, rawXsd.LastIndexOf(original)).ToString();

                rawXsd = new StringBuilder(rawXsd).Replace(original, substitute).ToString();
            }

            File.WriteAllText(outputPath, rawXsd);
        }

        private string CreateEnumSimpleType(string enumString, XmlSchema schema)
        {
            var acceptableValues = enumString.Split(',').Select(s => s.Trim()).ToList();

            var simpleType = new XmlSchemaSimpleType();
            var union = new XmlSchemaSimpleTypeUnion();
            union.MemberTypes = new XmlQualifiedName[] { new XmlQualifiedName("xmlLayout:mvvmPattern") };
            simpleType.Content = union;

            var nestedSimpleType = new XmlSchemaSimpleType();
            union.BaseTypes.Add(nestedSimpleType);

            var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = new XmlQualifiedName("xs:token") };
            nestedSimpleType.Content = restriction;

            foreach (var value in acceptableValues)
            {
                restriction.Facets.Add(new XmlSchemaEnumerationFacet { Value = value });
            }

            var enumName = "enum" + enumPatterns.Count;
            simpleType.Name = enumName;

            schema.Items.Add(simpleType);

            enumPatterns.Add(enumString, enumName);

            return enumName;
        }

        public void ProcessCustomAttributes()
        {
            var customAttributes = XmlLayoutUtilities.GetGroupedCustomAttributeNames();

            foreach (var group in customAttributes)
            {
                var existingAttributes = GetAttributeGroup(group.Key);

                foreach (var attributeName in group.Value)
                {
                    var _attributeName = Char.ToLower(attributeName[0]) + attributeName.Substring(1);
                    var attribute = XmlLayoutUtilities.GetCustomAttribute(attributeName);

                    if (attribute.RestrictToPermittedElementsOnly)
                    {
                        // add attribute to permitted elements
                        foreach(var element in attribute.PermittedElements)
                        {
                            if (!customAttributesToAddByElement.ContainsKey(element)) customAttributesToAddByElement.Add(element, new Dictionary<string, CustomXmlAttribute>());

                            customAttributesToAddByElement[element].Add(_attributeName, attribute);
                        }
                    }
                    else if (!existingAttributes.Contains(attributeName, StringComparer.OrdinalIgnoreCase))
                    {
                        // add attribute to group
                        if (!customAttributesToAddByGroup.ContainsKey(group.Key)) customAttributesToAddByGroup.Add(group.Key, new Dictionary<string, CustomXmlAttribute>());

                        customAttributesToAddByGroup[group.Key].Add(_attributeName, attribute);
                    }
                }
            }
        }

        public void ProcessCustomAttributeGroups()
        {
            var attributeGroupTypes = new List<Type>();

            var assemblies = XmlLayoutAssemblyHelper.GetAssemblyNames();

            var customXmlAttributeType = typeof(CustomXmlAttributeGroup);

            foreach (var assembly in assemblies)
            {
                try
                {
                    attributeGroupTypes.AddRange(Assembly.Load(assembly)
                                                         .GetTypes()
                                                         .Where(t => !t.IsAbstract && t.IsSubclassOf(customXmlAttributeType))
                                                         .ToList());
                }
                catch { }
            }

            // validate and filter out any invalid entries
            foreach (var attributeGroupType in attributeGroupTypes)
            {
                //var instance = (CustomXmlAttributeGroup)Activator.CreateInstance(attributeGroupType);\
                var instance = GetTestInstance<CustomXmlAttributeGroup>(attributeGroupType);
                if (instance.Validate())
                {
                    this.customAttributeGroupsToAdd.Add(instance);
                }
            }
        }

        public void ProcessCustomTags()
        {
            var tags = XmlLayoutUtilities.GetXmlTagHandlerNames();
            var groupedTags = new Dictionary<string, Dictionary<string, ElementTagHandler>>();
            foreach (var tag in tags)
            {
                var tagHandler = XmlLayoutUtilities.GetXmlTagHandler(tag);

                if (!tagHandler.isCustomElement) continue;

                if (!groupedTags.ContainsKey(tagHandler.elementGroup)) groupedTags.Add(tagHandler.elementGroup, new Dictionary<string, ElementTagHandler>());

                groupedTags[tagHandler.elementGroup].Add(tag, tagHandler);
            }

            foreach (var group in groupedTags)
            {
                var existingTags = GetElementGroup(group.Key);

                foreach (var tag in group.Value)
                {
                    if (!existingTags.Contains(tag.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!customElementTagHandlersToAdd.ContainsKey(group.Key)) customElementTagHandlersToAdd.Add(group.Key, new Dictionary<string, ElementTagHandler>());

                        customElementTagHandlersToAdd[group.Key].Add(tag.Key, tag.Value);
                    }
                }
            }
        }

        protected List<string> GetAttributeGroup(CustomXmlAttribute.eAttributeGroup attributeGroup)
        {
            var attributeGroupName = GetAttributeGroupName(attributeGroup);

            if (attributeGroupName == null) return new List<string>();

            var ag = schemaElement.Elements(ns + "attributeGroup")
                                  .First(s => {
                                      var name = s.Attribute("name");
                                      if (name != null)
                                      {
                                          return name.Value.Equals(attributeGroupName, StringComparison.OrdinalIgnoreCase);
                                      }

                                      return false;
                                  });

            return ag.Elements()
                     .Where(e => e.Name.LocalName == "attribute")
                     .Select(e => e.Attribute("name").Value)
                     .ToList();
        }

        protected List<string> GetElementGroup(string elementGroup)
        {
            var eg = schemaElement.Elements(ns + "group")
                                  .FirstOrDefault(s =>
                                  {
                                      var name = s.Attribute("name");
                                      if (name != null)
                                      {
                                          return name.Value.Equals(elementGroup, StringComparison.OrdinalIgnoreCase);
                                      }

                                      return false;
                                  });

            if (eg == null)
            {
                return new List<string>();
            }

            return eg.Element(ns + "choice")
                     .Elements()
                     .Where(e => e.Name.LocalName == "element")
                     .Select(e => e.Attribute("ref").Value)
                     .ToList();
        }

        protected string GetAttributeGroupName(CustomXmlAttribute.eAttributeGroup attributeGroup)
        {
            switch (attributeGroup)
            {
                case CustomXmlAttribute.eAttributeGroup.Animation: return "animation";
                case CustomXmlAttribute.eAttributeGroup.Button: return "button";
                case CustomXmlAttribute.eAttributeGroup.Events: return "events";
                case CustomXmlAttribute.eAttributeGroup.Image: return "image";
                case CustomXmlAttribute.eAttributeGroup.LayoutBase: return "layoutBase";
                case CustomXmlAttribute.eAttributeGroup.LayoutElement: return "layoutElement";
                case CustomXmlAttribute.eAttributeGroup.RectPosition: return "rectPosition";
                case CustomXmlAttribute.eAttributeGroup.RectTransform: return "rectTransform";
                case CustomXmlAttribute.eAttributeGroup.AllElements: return "simpleAttributes";
                case CustomXmlAttribute.eAttributeGroup.Text: return "text";
                case CustomXmlAttribute.eAttributeGroup.Dragging: return "dragging";
                case CustomXmlAttribute.eAttributeGroup.Tooltip: return "tooltip";
            }

            return null;
        }

        static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Debug.Log("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Debug.Log("ERROR: ");

            Debug.Log(args.Message);
        }

        private void AddElementToParent(XmlSchema schema, string parentElement, string childElement)
        {
            var parentSchemaElement = (XmlSchemaElement)schema.Elements[new XmlQualifiedName(parentElement, @"XmlLayout")];
            var items = ((XmlSchemaChoice)((XmlSchemaComplexContentExtension)((XmlSchemaComplexType)parentSchemaElement.SchemaType).ContentModel.Content).Particle).Items;

            items.Add(new XmlSchemaElement { RefName = new XmlQualifiedName(childElement, @"XmlLayout") });
        }
    }
}
