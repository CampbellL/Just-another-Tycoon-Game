using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UI.Xml
{
    [ExecuteInEditMode]
    public partial class XmlLayout : MonoBehaviour
    {
        [Tooltip("If this is set to true, then XmlLayout will preload some of its functionality in advance. This will mean that there will be a slight performance hit the first time an XmlLayout is loaded. Without the preload, there will be a minor performance hit each time a new Xml Tag type is parsed.")]
        public bool PreloadXmlLayoutCache = true;

        /// <summary>
        /// Reference to the Xml file used by this instance of XmlLayout, which will be used to populate the 'Xml' property. This is optional; you can manually specify Xml if you wish.
        /// </summary>
        public TextAsset XmlFile;

        [Tooltip("Automatically reload Xml file if it is changed? Note: This will override the Xml property, and it will only work in the Unity Editor.")]
        public bool AutomaticallyReloadXmlFileIfItChanges = true;

        [Tooltip("If set to true, this XmlLayout will automatically rebuild when Awake() is called. This should always be set if this XmlLayout loads data dynamically.")]
        public bool ForceRebuildOnAwake = true;

        [Tooltip("If set to true, this XmlLayout will automatically reload the Xml from the XmlFile when Awake() is called.")]
        public bool ForceReloadXmlFileOnAwake = false;

        [TextArea]
        public string Xml = "<XmlLayout>\r\n</XmlLayout>";

        [Tooltip("An optional list of Xml files which contain default values (such as element styles).")]
        public List<TextAsset> DefaultsFiles;

        /// <summary>
        /// Used by the editor to determine whether or not to trigger an update if a file which has been included (via the <Include> tag) has been updated
        /// </summary>
        [SerializeField, HideInInspector]
        public List<string> IncludedFiles = new List<string>();

        /// <summary>
        /// Used by the editor to determine whether or not to show the XML code
        /// </summary>
        public bool editor_showXml = false;

        public Vector2 editor_xmlScrollPosition = new Vector2();

        [SerializeField]
        private string previousXml = string.Empty;

        [SerializeField]
        public ElementDictionary ElementsById = new ElementDictionary();

        private XmlLayoutController _xmlLayoutController;
        /// <summary>
        /// This is a reference to the layout controller for this XmlLayout (if this XmlLayout doesn't use a controller, this will be null)
        /// </summary>
        public XmlLayoutController XmlLayoutController
        {
            get
            {
                if (_xmlLayoutController == null)
                {
                    _xmlLayoutController = this.GetComponent<XmlLayoutController>();
                }

                return _xmlLayoutController;
            }
        }

        private XmlElement m_XmlElement;
        /// <summary>
        /// This is a reference to the 'XmlElement' object of this XmlLayout
        /// </summary>
        public XmlElement XmlElement
        {
            get
            {
                if (m_XmlElement == null) InitialiseXmlElement();

                return m_XmlElement;
            }
        }

        /// <summary>
        /// Parent XmlLayout of this XmlLayout (if there is one)
        /// </summary>
        public XmlLayout ParentLayout { get; internal set; }

        private void InitialiseXmlElement()
        {
            if (m_XmlElement == null)
            {
                m_XmlElement = this.GetComponent<XmlElement>();
            }

            if (m_XmlElement == null)
            {
                m_XmlElement = this.gameObject.AddComponent<XmlElement>();
                m_XmlElement.Initialise(this, this.transform as RectTransform, XmlLayoutUtilities.GetXmlTagHandler("XmlLayout"));
            }
        }

        [SerializeField]
        public DefaultAttributeValueDictionary defaultAttributeValues = new DefaultAttributeValueDictionary();

        private bool m_awake = false;
        public bool IsReady { get; protected set; }

        protected XmlLayoutTooltip m_Tooltip;
        public XmlLayoutTooltip Tooltip
        {
            get
            {
                if (m_Tooltip == null) CreateTooltipObject();

                return m_Tooltip;
            }
        }

        // Used to store default state values
        [SerializeField]
        protected AttributeDictionary m_defaultTooltipAttributes = new AttributeDictionary();

        public bool rebuildInProgress { get; private set; }
        protected bool rebuildScheduled { get; private set; }

        public ColorDictionary namedColors = new ColorDictionary();

#if TEXTMESHPRO_PRESENT
        public MaterialDictionary textMeshProMaterials = new MaterialDictionary();
#endif

        [SerializeField]
        protected bool m_useUnscaledTime = true;
        public bool UseUnscaledTime
        {
            get { return m_useUnscaledTime; }
            internal set { m_useUnscaledTime = value; }
        }

        public List<string> ChildElementXmlFiles = new List<string>();

        void Awake()
        {
            m_awake = true;

#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this.gameObject))
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(this.gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
#endif

            if (Application.isPlaying)
            {
                if (PreloadXmlLayoutCache) HandlePreload();

                if (ForceRebuildOnAwake)
                {
                    if (XmlFile != null && ForceReloadXmlFileOnAwake) ReloadXmlFile();
                    else RebuildLayout(true);
                }

                var selectableNavigator = GameObject.FindObjectOfType<XmlLayoutSelectableNavigator>();

                if (selectableNavigator != null)
                {
                    if (selectableNavigator.gameObject == this.gameObject)
                    {
                        DestroyImmediate(selectableNavigator);
                        selectableNavigator = null;
                    }
                }


                if (selectableNavigator == null)
                {
                    CreateSelectableNavigator();
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (XmlFile != null)
                {
                    if (Xml != XmlFile.text)
                    {
                        Debug.LogFormat("[XmlLayout] : '{0}' has changed - reloading file and rebuilding layout.", XmlFile.name);

                        ReloadXmlFile();

                        // Calling MarkSceneDirty here doesn't seem to work, but delaying the call to the end of the frame does
                        XmlLayoutTimer.AtEndOfFrame(() => UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene), this);
                    }
                }
            }
#endif

            if (Application.isPlaying && !ForceRebuildOnAwake)
            {
                SetupElementEventHandlers();
/*                if (XmlLayoutController != null)
                {
                    XmlLayoutTimer.DelayedCall(0.1f, () => XmlLayoutController.LayoutRebuilt(ParseXmlResult.Changed), this);
                }*/
            }

            IsReady = true;
        }

        public void ReloadXmlFile()
        {
            if (XmlFile != null)
            {
                Xml = XmlFile.text;
                RebuildLayout(true);
            }
        }

        public void LoadXmlFromUrl(string url)
        {
            XmlLayoutWebUtilities.LoadTextFromUrl((xml) =>
            {
                Xml = xml;
                RebuildLayout(true);
            }, url);
        }

        void CreateTooltipObject()
        {
            var prefab = XmlLayoutUtilities.LoadResource<GameObject>("XmlLayout Prefabs/Tooltip");
            m_Tooltip = ((GameObject)Instantiate(prefab)).GetComponent<XmlLayoutTooltip>();
            m_Tooltip.transform.SetParent(this.transform);
            m_Tooltip.transform.localPosition = Vector3.zero;
            m_Tooltip.transform.localScale = Vector3.one;
            m_Tooltip.name = "Tooltip";
            m_Tooltip.gameObject.SetActive(false);
        }

        void _Destroy(GameObject o)
        {
            if (o == null) return;

            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }

        void CreateSelectableNavigator()
        {
            var go = new GameObject("XmlLayoutSelectableNavigator", typeof(XmlLayoutSelectableNavigator));
            go.transform.SetAsLastSibling();
        }

        void ClearContents()
        {
            if (this == null) return;

            for (var x = transform.childCount - 1; x >= 0; x--)
            {
                var t = transform.GetChild(x);

                t.transform.SetParent(null);

                _Destroy(t.gameObject);
            }

            IncludedFiles.Clear();
            ElementsById.Clear();
#if !ENABLE_IL2CPP && MVVM_ENABLED
            ElementDataSources.Clear();
#endif
            defaultAttributeValues.Clear();
            cachedPotentialSelectors.Clear();
            animations.Clear();

            if (m_Tooltip != null) _Destroy(m_Tooltip.gameObject);
        }

        /// <summary>
        /// Clear the contents of this XmlLayout and rebuild it (using the Xml value)
        /// Call this after changing the Xml value for changes to take effect
        /// </summary>
        public void RebuildLayout(bool forceEvenIfXmlUnchanged = false, bool throwExceptionIfXmlIsInvalid = false)
        {
            if (!forceEvenIfXmlUnchanged && (!this.gameObject.activeInHierarchy || !m_awake)) return;
            if (rebuildInProgress) return;

            rebuildInProgress = true;

            // Clear the child collections
            if (forceEvenIfXmlUnchanged || !previousXml.Equals(Xml))
            {
                XmlElement.childElements.Clear();
                ChildElementXmlFiles.Clear();

                //defaultAttributeValues.Clear();
                //defaultAttributesMergedCache.Clear();
                //namedColors.Clear();
            }

            try
            {
                var parseResult = ParseXml(null, true, true, forceEvenIfXmlUnchanged, throwExceptionIfXmlIsInvalid);


                // Notify the XmlLayoutController that the Layout has been rebuilt
                if (XmlLayoutController != null)
                {
                    XmlLayoutController.ViewModelUpdated(false);
                    XmlLayoutController.NotifyXmlElementReferencesOfLayoutRebuild();
                    XmlLayoutController.PreLayoutRebuilt();
                    XmlLayoutController.LayoutRebuilt(parseResult);
                    XmlLayoutController.PostLayoutRebuilt();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && parseResult != ParseXmlResult.Unchanged)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
                }
#endif
            }
            finally
            {
                rebuildInProgress = false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xmlToParse">The xml to parse. If this argument is null, then this method will use this XmlLayout's 'Xml' property instead.</param>
        /// <param name="clearContents">Should the contents of this XmlLayout be cleared before parsing the xml?</param>
        /// <param name="loadDefaultsFiles">Should the defaults files (if any) be loaded before parsing the xml?</param>
        /// <param name="forceEvenIfXmlUnchanged">Should the xml be parsed if it hasn't changed? (only applicable if xmlToParse is null)</param>
        /// <param name="throwExceptionIfXmlIsInvalid">Should an exception be thrown if the Xml contains errors?</param>
        ParseXmlResult ParseXml(string xmlToParse = null,
                                bool clearContents = true,
                                bool loadDefaultsFiles = true,
                                bool forceEvenIfXmlUnchanged = false,
                                bool throwExceptionIfXmlIsInvalid = false)
        {
            if (xmlToParse == null)
            {
                if (!forceEvenIfXmlUnchanged)
                {
                    // If our Xml hasn't changed, and we aren't required to force update, then don't continue
                    // Note: this only happens if xmlToParse is null (which means we are parsing the primary Xml of this XmlLayout)
                    if (previousXml.Equals(Xml)) return ParseXmlResult.Unchanged;
                }

                previousXml = Xml;

                xmlToParse = Xml;
            }

            if (XmlLayoutController != null)
            {
                xmlToParse = XmlLayoutController.ProcessViewModel(xmlToParse);
            }

            if (LocalizationFile != null)
            {
                xmlToParse = HandleLocalization(xmlToParse);
            }

            if (clearContents)
            {
                ClearContents();
            }

            if (loadDefaultsFiles && DefaultsFiles != null)
            {
                defaultAttributeValues.Clear();

                DefaultsFiles.ForEach(f =>
                {
                    if (f != null) ParseXml(f.text, false, false, true);
                });
            }

            var rectTransform = this.transform as RectTransform;

            using (var stringReader = new StringReader(xmlToParse))
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                };

                using (var reader = XmlReader.Create(stringReader, settings))
                {
                    reader.ReadToFollowing("XmlLayout");

                    ParseNode(reader, rectTransform, rectTransform, true);
                }
            }

            return ParseXmlResult.Changed;
        }

        private string CleanupTextAttribute(string text)
        {
            text = text.Trim();

            // Strip out any instances of multiple spaces (as in HTML)
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            while (text.Contains("\r\n "))
            {
                text = text.Replace("\r\n ", "\r\n");
            }

            return text;
        }

        private Dictionary<string, List<KeyValuePair<string, AttributeDictionary>>> cachedPotentialSelectors = new Dictionary<string, List<KeyValuePair<string, AttributeDictionary>>>();

        internal List<KeyValuePair<string, AttributeDictionary>> GetPotentialSelectors(string elementType, string _class)
        {
            string cacheKey = string.Format("{0}>{1}", elementType, _class);

            if (cachedPotentialSelectors.ContainsKey(cacheKey))
            {
                return cachedPotentialSelectors[cacheKey];
            }

            var potentialSelectors = new List<KeyValuePair<string, AttributeDictionary>>();

            // OrdinalIgnoreCase string comparison doesn't seem to work properly in the Web Player, which is why we're calling ToLower() here
            string elementTypeSelector = string.Format("@{0}", elementType).ToLower();

            if (defaultAttributeValues.ContainsKey(elementType))
            {
                // This gets called quite a lot; replacing LINQ with foreach approach to reduce GC issues
                /*potentialSelectors = defaultAttributeValues[elementType]
                                     .Where(k => k.Key.EndsWith(_class, StringComparison.OrdinalIgnoreCase) || k.Key.EndsWith(elementTypeSelector, StringComparison.OrdinalIgnoreCase))
                                     .ToList();                  */

                foreach (var potentialSelector in defaultAttributeValues[elementType])
                {
                    if (potentialSelector.Key.EndsWith(_class, StringComparison.OrdinalIgnoreCase)
                     || potentialSelector.Key.EndsWith(elementTypeSelector, StringComparison.OrdinalIgnoreCase))
                    {
                        potentialSelectors.Add(potentialSelector);
                    }
                }
            }

            cachedPotentialSelectors.Add(cacheKey, potentialSelectors);

            return potentialSelectors;
        }

        private Regex SelectorSplitter = new Regex(@"(?<=[>:])");

        private Dictionary<string, string[]> cachedSelectorParts = new Dictionary<string, string[]>();

        internal string[] GetSelectorParts(string selector)
        {
            // This may be called many times for the same selector; caching the results avoids unnecessary Regex calls
            if (cachedSelectorParts.ContainsKey(selector)) return cachedSelectorParts[selector];

            var parts = SelectorSplitter.Split(selector);

            // More GC-friendly than LINQ Reverse().ToArray()
            Array.Reverse(parts);

            cachedSelectorParts.Add(selector, parts);

            return parts;
        }

        internal string GetAttributeValueForNode_IncludingDefaults(string type,
                                                                   AttributeDictionary attributes,
                                                                   string attributeName,
                                                                   XmlElement parentElement)
        {
            var attributeValue = attributes.GetValue(attributeName);

            if (String.IsNullOrEmpty(attributeValue))
            {
                if (defaultAttributeValues.ContainsKey(type))
                {
                    // check 'all'
                    if (defaultAttributeValues[type].ContainsKey("all")) attributeValue = defaultAttributeValues[type]["all"].GetValue(attributeName);

                    var classes = attributes.ContainsKey("class") ? attributes["class"].ToClassList() : new List<string>();

                    classes.Remove("all");
                    classes.Insert(0, "all");

                    foreach (var _class in classes)
                    {
                        var potentialSelectors = GetPotentialSelectors(type, _class);

                        foreach (var potentialSelector in potentialSelectors)
                        {
                            // don't bother executing the rest of this block if the potential selector doesn't have the attribute in question
                            //Debug.Log(attributeName);
                            //Debug.Log(potentialSelector);
                            //Debug.Log(potentialSelector.Value);
                            if (!potentialSelector.Value.ContainsKey(attributeName)) continue;

                            bool matches = true;
                            var parts = GetSelectorParts(potentialSelector.Key);

                            if (parts.Count() == 1)
                            {
                                var part = parts[0].StripChars('>', ':');
                                if (!part.Equals(_class, StringComparison.OrdinalIgnoreCase)) matches = false;
                            }
                            else
                            {
                                bool first = true;
                                XmlElement currentElement = null;
                                var currentElementTagType = type;

                                foreach (var part in parts)
                                {
                                    var _part = part.StripChars('>', ':');
                                    bool searchByType = _part.StartsWith("@");
                                    if (searchByType) _part = _part.Substring(1);

                                    if (part.EndsWith(">"))
                                    {
                                        currentElement = currentElement == null ? currentElement = parentElement : currentElement.parentElement;
                                    }
                                    else if (part.EndsWith(":"))
                                    {
                                        bool parentFound = false;
                                        bool _first = true;

                                        while (!parentFound && (_first || currentElement != null))
                                        {
                                            currentElement = _first && currentElement == null ? currentElement = parentElement : currentElement.parentElement;

                                            if (currentElement != null)
                                            {
                                                if (currentElement != null) currentElementTagType = currentElement.tagType;

                                                if (searchByType)
                                                {
                                                    parentFound = currentElementTagType.Equals(_part, StringComparison.OrdinalIgnoreCase);
                                                }
                                                else
                                                {
                                                    parentFound = currentElement.HasClass(_part);
                                                }
                                            }

                                            _first = false;
                                        }
                                    }

                                    if (!first && currentElement == null)
                                    {
                                        matches = false;
                                        break;
                                    }

                                    if (currentElement != null) currentElementTagType = currentElement.tagType;

                                    if (searchByType)
                                    {
                                        if (!currentElementTagType.Equals(_part, StringComparison.OrdinalIgnoreCase)) matches = false;
                                    }
                                    else
                                    {
                                        if (first)
                                        {
                                            // for the first iteration, we may not have an element yet
                                            // check our classes collection instead
                                            if (!classes.Contains(_part)) matches = false;
                                        }
                                        else
                                        {
                                            if (currentElement != null && !currentElement.HasClass(_part)) matches = false;
                                        }
                                    }

                                    first = false;
                                }
                            }

                            if (matches) attributeValue = potentialSelector.Value.GetValue(attributeName);
                        }
                    }
                }
            }

            return attributeValue;
        }

        // Caching this so as to avoid constantly creating a new attribute dictionary for each call
        private AttributeDictionary defaultAttributesMergedCache = new AttributeDictionary();

        internal AttributeDictionary MergeDefaultAttributesWithElementAttributes(XmlElement xmlElement,
                                                                                 string elementType,
                                                                                 AttributeDictionary elementAttributes)
        {
            if (xmlElement == null) return elementAttributes;

            defaultAttributesMergedCache.Clear();
            AttributeDictionary defaultAttributesMerged = defaultAttributesMergedCache;
            AttributeDictionary mergedAttributes = elementAttributes;

            if (defaultAttributeValues.ContainsKey(elementType))
            {
                if (defaultAttributeValues[elementType].ContainsKey("all"))
                {
                    defaultAttributesMerged = defaultAttributeValues[elementType]["all"].Clone();
                }

                var classesFromAttribute = elementAttributes.ContainsKey("class") ? elementAttributes["class"].ToClassList() : new List<string>();
                var classesFromElement = xmlElement.classes;

                var classes = classesFromAttribute;
                classes.AddRange(classesFromElement);
                classes.Remove("all");

                if (classes.Count > 1)
                {
                    if (defaultAttributeValues.ContainsKey(elementType))
                    {
                        classes = classes.Distinct()
                                         .OrderBy(c => defaultAttributeValues[elementType].order.ContainsKey(c) ? defaultAttributeValues[elementType].order[c] : int.MaxValue)
                                         .ToList();
                    }
                }

                classes.Insert(0, "all");

                foreach (var _class in classes)
                {
                    var potentialSelectors = GetPotentialSelectors(elementType, _class);

                    foreach (var potentialSelector in potentialSelectors)
                    {
                        bool matches = true;
                        var parts = GetSelectorParts(potentialSelector.Key);

                        if (parts.Count() == 1)
                        {
                            // if there's only one part, try for a 1:1 match
                            var part = parts[0].StripChars('>', ':');

                            if (!part.Equals(_class, StringComparison.OrdinalIgnoreCase)) matches = false;
                        }
                        else
                        {
                            // we need to ascend through the element tree and see if this is element is a valid match
                            // the parts array is already in reverse order, so we'll start at the 'beginning' to see if each part is a match
                            var currentElement = xmlElement;

                            foreach (var part in parts)
                            {
                                var _part = part.StripChars('>', ':');
                                bool searchByType = _part.StartsWith("@");
                                if (searchByType) _part = _part.Substring(1);

                                if (part.EndsWith(">"))
                                {
                                    // ascend one
                                    currentElement = currentElement.parentElement;
                                }
                                else if (part.EndsWith(":"))
                                {
                                    // ascend till we find a match (or match fails)
                                    bool parentFound = false;
                                    while (!parentFound && currentElement != null)
                                    {
                                        currentElement = currentElement.parentElement;

                                        if (currentElement != null)
                                        {
                                            if (searchByType)
                                            {
                                                parentFound = currentElement.tagType.Equals(_part, StringComparison.OrdinalIgnoreCase);
                                            }
                                            else
                                            {
                                                parentFound = currentElement.HasClass(_part);
                                            }
                                        }
                                    }
                                }

                                if (currentElement == null)
                                {
                                    matches = false;
                                    break;
                                }

                                if (searchByType)
                                {
                                    if (!currentElement.tagType.Equals(_part, StringComparison.OrdinalIgnoreCase))
                                    {
                                        matches = false;
                                    }
                                }
                                else
                                {
                                    if (!currentElement.HasClass(_part))
                                    {
                                        matches = false;
                                    }
                                }
                            }
                        }

                        if (matches)
                        {
                            // merge in the attributes

                            //defaultAttributesMerged = XmlLayoutUtilities.MergeAttributes(defaultAttributesMerged, potentialSelector.Value);
                            defaultAttributesMerged.Merge(potentialSelector.Value);
                        }
                    }
                }

                // merge the default attributes with the element attributes
                mergedAttributes = XmlLayoutUtilities.MergeAttributes(defaultAttributesMerged, elementAttributes);
            }

            return mergedAttributes;
        }

        /// <summary>
        /// Does the provided element match the specified pattern?
        /// (A pattern is a subset of a class selector)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        bool ElementMatchesSelectorPattern(XmlElement element, string pattern)
        {
            if (pattern.StartsWith("@"))
            {
                return (element.tagType == pattern.Replace("@", string.Empty));
            }
            else
            {
                return element.HasClass(pattern);
            }
        }

        void LoadIncludeFile(string path)
        {
            var xmlFile = Resources.Load(path) as TextAsset;
            if (xmlFile == null)
            {
                Debug.LogError("[XmlLayout] Unable to locate xml file using path '" + path + "'. Please ensure that the file is located within a Resources folder.");
                return;
            }

            ParseXml(xmlFile.text, false, false, true);

            if (!IncludedFiles.Contains(path))
            {
                IncludedFiles.Add(path);
            }
        }

        void LoadInlineIncludeFile(string path, RectTransform parent)
        {
            //var path = node.Attributes["path"].Value;

            // strip out the file extension, if provided
            path = path.Replace(".xml", string.Empty);

            var xmlFile = XmlLayoutUtilities.LoadResource<TextAsset>(path);

            if (xmlFile == null)
            {
                Debug.LogError(String.Format("[XmlLayout][{0}] Error locating include file : '{1}'.", this.name, path));
                return;
            }

            ChildElementXmlFiles.Add(path);

            using (var stringReader = new StringReader(xmlFile.text))
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                };

                using (var reader = XmlReader.Create(stringReader, settings))
                {
                    // Ignore the '<XmlLayout>' element
                    reader.MoveToContent();

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                subtree.Read();
                                ParseNode(subtree, parent);
                            }
                        }
                    }
                }
            }
        }

        internal void ParseNode(XmlReader reader,
                                RectTransform parent,
                                RectTransform element = null,
                                bool parseChildren = true,
                                XmlElement parentXmlElement = null)
        {
            var type = reader.Name;

            if (type.Equals("Defaults", StringComparison.OrdinalIgnoreCase))
            {
                HandleDefaults(reader);
                return;
            }

            AttributeDictionary attributes = reader.GetAttributeDictionary();

            if (type.Equals("Include", StringComparison.OrdinalIgnoreCase))
            {
                LoadInlineIncludeFile(attributes.GetValue("path"), parent);
                return;
            }

            //Debug.Log(type + " " + reader.NodeType + " " + attributes);
            var tagHandler = XmlLayoutUtilities.GetXmlTagHandler(type);
            if (tagHandler == null) return;

            tagHandler.SetInstance(element, this);

            XmlElement xmlElement = null;
            if (element == null)
            {
                string prefabPath = GetAttributeValueForNode_IncludingDefaults(type, attributes, "prefabPath", parentXmlElement);

                xmlElement = tagHandler.GetInstance(parent, this, prefabPath);

                if (parentXmlElement != null)
                {
                    parentXmlElement.AddChildElement(xmlElement, false);
                }
            }

            var tagTransform = element ?? xmlElement.rectTransform;
            tagHandler.SetInstance(tagTransform, this);

            if (xmlElement != null)
            {
                xmlElement.attributes = attributes;
#if !ENABLE_IL2CPP && MVVM_ENABLED
                xmlElement.DataSource = xmlElement.GetAttribute("vm-dataSource");
#endif
            }

            tagHandler.Open(attributes);

            if (tagHandler.UseParseChildElements)
            {
                // if the tag handler successfully parses the child nodes, then don't attempt to parse them here
                // (this is only used for specific elements, e.g. Dropdown)

                reader.MoveToContent();
                var xmlNode = new XmlDocument().ReadNode(reader);
                tagHandler.ParseChildElements(xmlNode);

                parseChildren = false;
            }

            if (parseChildren)
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        using (var subtree = reader.ReadSubtree())
                        {
                            subtree.Read();

                            tagHandler.SetInstance(tagTransform, this);
                            ParseNode(subtree, tagHandler.transformToAddChildrenTo, null, true, xmlElement);
                        }
                    }
                    else
                    {
                        if (reader.NodeType != XmlNodeType.EndElement)
                        {
                            attributes["text"] = CleanupTextAttribute(reader.ReadContentAsString());
                        }
                    }
                }
            }

            tagHandler.SetInstance(tagTransform, this);

            if (attributes.ContainsKey("id"))
            {
                if (ElementsById.ContainsKey(attributes["id"]))
                {
                    Debug.LogError("[XmlLayout] Ignoring duplicate id value '" + attributes["id"] + ". Id values must be unique.");
                }
                else
                {
                    if (xmlElement != null)
                    {
                        ElementsById.Add(attributes["id"], xmlElement);
                    }
                }
            }

            // Preserve which attributes have been set manually (and aren't derived from a class)
            if (xmlElement != null) xmlElement.elementAttributes = attributes.Keys.ToList();

            attributes = MergeDefaultAttributesWithElementAttributes(xmlElement, type, attributes);

            if (xmlElement != null)
            {
                if (xmlElement.HasAttribute("class")) xmlElement.classes = xmlElement.GetAttribute("class").ToClassList();

                if (attributes.ContainsKey("hoverClass"))
                {
                    var hoverClasses = attributes["hoverClass"].Split(',', ' ').Select(s => s.Trim().ToLower()).Where(c => !String.IsNullOrEmpty(c)).ToList();
                    xmlElement.hoverClasses = hoverClasses;
                }

                if (attributes.ContainsKey("selectClass"))
                {
                    var selectClasses = attributes["selectClass"].Split(',', ' ').Select(s => s.Trim().ToLower()).Where(c => !String.IsNullOrEmpty(c)).ToList();
                    xmlElement.selectClasses = selectClasses;
                }

                if (attributes.ContainsKey("pressClass"))
                {
                    var pressClasses = attributes["pressClass"].Split(',', ' ').Select(s => s.Trim().ToLower()).Where(c => !String.IsNullOrEmpty(c)).ToList();
                    xmlElement.pressClasses = pressClasses;
                }

                xmlElement.ApplyAttributes(attributes);
            }
            else
            {
                tagHandler.ApplyAttributes(attributes);
            }

            tagHandler.Close();

            if (!tagHandler.renderElement)
            {
                tagHandler.RemoveElement();
            }
        }

        void HandleDefaults(XmlReader defaultsReader)
        {
            while (defaultsReader.Read())
            {
                if (defaultsReader.IsStartElement())
                {
                    var elementType = defaultsReader.Name;

                    AttributeDictionary attributes = defaultsReader.GetAttributeDictionary();

                    switch (elementType)
                    {
                        case "Color":
                            HandleColorNode(attributes);
                            break;

                        case "Tooltip":
                            HandleDefaultTooltipNode(attributes);
                            break;

#if TEXTMESHPRO_PRESENT
                        case "TextMeshProMaterial":
                            HandleTextMeshProMaterialNode(attributes);
                            break;
#endif

                        case "Animation":
                            HandleAnimationNode(attributes);
                            break;

                        default:
                            HandleDefaultNode(elementType, attributes);
                            break;
                    }
                }
            }
        }

        void HandleDefaultTooltipNode(AttributeDictionary attributes)
        {
            m_defaultTooltipAttributes = attributes;
        }

        void HandleDefaultNode(string type, AttributeDictionary attributes)
        {
            if (XmlLayoutUtilities.GetXmlTagHandler(type) == null) return;

            //.Split(',', ' ').Select(s => s.Trim().ToLower()).ToList()
            var classes = attributes.ContainsKey("class") ? attributes["class"].ToClassList() : new List<string>() { "all" };

            foreach (var _class in classes)
            {
                if (!defaultAttributeValues.ContainsKey(type))
                {
                    defaultAttributeValues.Add(type, new ClassAttributeCollectionDictionary());
                }

                if (!defaultAttributeValues[type].ContainsKey(_class))
                {
                    defaultAttributeValues[type].Add(_class, new AttributeDictionary());
                }

                defaultAttributeValues[type][_class] = XmlLayoutUtilities.MergeAttributes(defaultAttributeValues[type][_class], attributes);
                defaultAttributeValues[type][_class].Remove("class");
            }
        }

#if TEXTMESHPRO_PRESENT
        void HandleTextMeshProMaterialNode(AttributeDictionary attributes)
        {
            var material = Tags.TextMeshProMaterialTagHandler.CreateMaterial(this, attributes);

            if (material != null)
            {
                textMeshProMaterials.SetValue(attributes["name"], material);
            }
        }
#endif

        void HandleColorNode(AttributeDictionary attributes)
        {
            if (!(attributes.ContainsKey("name") && attributes.ContainsKey("color")))
            {
                Debug.LogWarning("[XmlLayout] Warning: Named Color tag without a name and/or color - both are required.");
                return;
            }

            if (namedColors.ContainsKey(attributes["name"]))
            {
                namedColors[attributes["name"]] = attributes["color"].ToColor(this);
            }
            else
            {
                namedColors.Add(attributes["name"], attributes["color"].ToColor(this));
            }
        }

        /// <summary>
        /// Return a specific element within this XmlLayout
        /// Note: the element must have the id attribute set in order to use this function
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XmlElement GetElementById(string id)
        {
            if (ElementsById.ContainsKey(id))
            {
                return ElementsById[id];
            }

            return null;
        }

        /// <summary>
        /// Return the component of a specific element within this XmlLayout.
        /// Note   : the element must have the id attribute set in order to use this function
        /// Note 2 : the component must exist on the target element
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetElementById<T>(string id)
        {
            if (ElementsById.ContainsKey(id))
            {
                var t = ElementsById[id];
                var component = t.GetComponent<T>();

                if (component != null) return component;
            }

            return default(T);
        }

        public List<XmlElement> GetElementsByClass(string _class)
        {
            return XmlElement.GetChildElementsWithClass(_class);
        }

        /// <summary>
        /// Get the string id (if any) of a RectTRansform element in this XmlLayout
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetElementId(RectTransform element)
        {
            if (ElementsById.Any(e => e.Value.rectTransform == element))
            {
                return ElementsById.First(kvp => kvp.Value.rectTransform == element).Key;
            }

            return null;
        }

        /// <summary>
        /// Get the value of all child XmlElements by element id
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetFormData()
        {
            return XmlElement.GetFormData(UI.Xml.XmlElement.eLocateElementsBy.Id);
        }

        /*void OnRectTransformDimensionsChange()
        {
            if (!gameObject.activeInHierarchy) return;

            if (!XmlElement.IsAnimating)
            {
                // RebuildLayoutDelayed will only execute once, even if OnRectTransformDimensionsChanged() is called multiple times in one frame
                RebuildLayoutDelayed();
            }
        }*/

        void RebuildLayoutDelayed()
        {
            if (rebuildScheduled) return; // don't rebuild more than once at a time

            rebuildScheduled = true;

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                try
                {
                    RebuildLayout(true);
                }
                finally
                {
                    rebuildScheduled = false;
                }
            }, this);
        }

        public void Show(Action onCompleteCallback = null, bool forceEvenIfVisible = false)
        {
            XmlElement.Show(false, onCompleteCallback, forceEvenIfVisible);
        }

        public void Hide(Action onCompleteCallback = null, bool forceEvenIfNotVisible = false)
        {
            XmlElement.Hide(false, onCompleteCallback, forceEvenIfNotVisible);
        }

        void HandlePreload()
        {
            var preloader = this.GetComponent<XmlLayoutPreloader>();
            if (preloader == null) preloader = GameObject.FindObjectOfType<XmlLayoutPreloader>();
            if (preloader == null)
            {
                // Only call Preload if we actually had to create the preload instance
                preloader = this.gameObject.AddComponent<XmlLayoutPreloader>();
                preloader.Preload();
            }
        }


        protected XmlElement m_CurrentTooltipElement;
        public void ShowTooltip(XmlElement element, string tooltipContent)
        {
            m_CurrentTooltipElement = element;

            if (ParentLayout != null)
            {
                ParentLayout.ShowTooltip(element, tooltipContent);
                return;
            }

            XmlLayoutTimer.DelayedCall(Tooltip.showDelayTime, () =>
            {
                if (m_CurrentTooltipElement != element) return;

                var _defaultAttributes = XmlLayoutUtilities.MergeAttributes(m_defaultTooltipAttributes, element.attributes);

#if TEXTMESHPRO_PRESENT
                _defaultAttributes.AddIfKeyNotExists("tooltipFontSize", "12");
                _defaultAttributes.AddIfKeyNotExists("tooltipTextColor", "rgb(1,1,1)");
                _defaultAttributes.AddIfKeyNotExists("tooltipUseTextMeshPro", "false");
#endif

                Tooltip.LoadAttributes(_defaultAttributes);

                Tooltip.currentElement = element;

                Tooltip.FadeIn();
                //Tooltip.gameObject.SetActive(true);

                Tooltip.SetText(tooltipContent);

                //Tooltip.SetStylesFromXmlElement(element);

                if (!Tooltip.followMouse)
                {
                    Tooltip.SetPositionAdjacentTo(element);

                    // the size/etc. of the tooltip may change as a result of text and styles, but it appears that the rectTransform values will not be updated until the the end of the current frame
                    // as such, we need to call SetTooltipPositionAdjacentTo again in one frame, just in case. This is primarily so that the tooltip will be clamped within the canvas area.
                    XmlLayoutTimer.AtEndOfFrame(() => { Tooltip.SetPositionAdjacentTo(element); }, this);
                }
            }, this);
        }

        internal void NotifyElementHidden(XmlElement element)
        {
            if (element == m_CurrentTooltipElement) HideTooltip(element);
        }

        public void HideTooltip(XmlElement sourceElement)
        {
            if (ParentLayout != null)
            {
                ParentLayout.HideTooltip(sourceElement);
            }

            if (sourceElement == m_CurrentTooltipElement)
            {
                Tooltip.FadeOut();

                m_CurrentTooltipElement = null;
                //Tooltip.gameObject.SetActive(false);
            }
        }

        private void SetupElementEventHandlers()
        {
            SetupElementEventHandlers(XmlElement);
        }

        private void SetupElementEventHandlers(XmlElement element)
        {
            element.childElements = element.childElements.Where(c => c != null).ToList() ?? new List<XmlElement>();

            foreach (var childElement in element.childElements)
            {
                SetupElementEventHandlers(childElement);
            }

            element.tagHandler.SetInstance(element);
            element.tagHandler.ApplyEventAttributes();
        }

    }
}
