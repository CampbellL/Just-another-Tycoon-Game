/*
 * This is the base tag handler class for all XmlLayout tags.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UI.Xml
{
    public abstract partial class ElementTagHandler
    {
        public virtual MonoBehaviour primaryComponent { get { return null; } }
        public RectTransform currentInstanceTransform { get; protected set; }
        public XmlLayout currentXmlLayoutInstance { get; protected set; }

        /// <summary>
        /// Gets a reference to the 'Image' component of the current element (if one exists)
        /// </summary>
        protected virtual Image imageComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Image>();
            }
        }

        /// <summary>
        /// Gets a reference to the 'LayoutElement' of the current element (if one exists)
        /// </summary>
        protected LayoutElement layoutElement
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<LayoutElement>();
            }
        }

        /// <summary>
        /// Gets a reference to the current Xml Element
        /// </summary>
        protected XmlElement currentXmlElement
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<XmlElement>();
            }
        }

        /// <summary>
        /// Defines which RectTransform children of this tag should be added to
        /// (By default, children are added directly to this RectTransform, but some tags, such as ScrollViews override this behaviour)
        /// </summary>
        public virtual RectTransform transformToAddChildrenTo
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform;
            }
        }

        protected EventTrigger eventTrigger
        {
            get
            {
                return currentXmlElement.EventTrigger;
            }
        }

        private List<string> _eventAttributeNames = new List<string>()
        {
            "onClick",
            "onMouseEnter",
            "onMouseExit",
            "onElementDropped",
            "onBeginDrag",
            "onEndDrag",
            "onDrag",
            "onSubmit",
            "onShow",
            "onHide",
            "onMouseUp",
            "onMouseDown"
        };

        protected List<string> mouseEventNames = new List<string>()
        {
            "onClick",
            "onMouseEnter",
            "onMouseExit",
            "onMouseUp",
            "onMouseDown"
        };

        /// <summary>
        /// Defines a list of attribute names that are considered to be 'Events'
        /// Any attribute listed here will be handled by the 'HandleEventAttribute' method rather than 'ApplyAttributes'
        /// </summary>
        protected virtual List<string> eventAttributeNames
        {
            get
            {
                return _eventAttributeNames;
            }
        }

        private string _prefabPath = null;

        /// <summary>
        /// Specifies a path to the prefab used by this tag
        /// By default, XmlLayout will look for a prefab in the 'XmlLayout Prefabs' folder with the same name as this class, dropping 'TagHandler'
        /// You can override this value to provide a custom path, or make it return null to avoid using a prefab and start from scratch
        /// </summary>
        public virtual string prefabPath
        {
            get
            {
                if (_prefabPath == null) _prefabPath = string.Format("XmlLayout Prefabs/{0}", this.GetType().Name.Replace("TagHandler", String.Empty));

                return _prefabPath;
            }
        }

        protected string _elementName;
        /// <summary>
        /// Returns the tag name of this element
        /// </summary>
        public string tagType
        {
            get
            {
                if (_elementName == null) _elementName = XmlLayoutUtilities.GetTagName(GetType());

                return _elementName;
            }
        }

        /// <summary>
        /// This determines which elements this element may be a child of
        /// default - Default behaviour, may be a child of any other element (with a few exceptions)
        /// V1.1 : This is now a string, so you can use whatever value you wish
        /// </summary>
        public virtual string elementGroup
        {
            get
            {
                return "default";
            }
        }

        /// <summary>
        /// This determines which element types may be a child of _this_ element
        /// default - Any element which is part of the 'Default' group (see ElementTagHandler.elementGroup) may be a child of this element
        /// </summary>
        public virtual string elementChildType
        {
            get
            {
                return "default";
            }
        }

        /// <summary>
        /// If this returns false, then no auto-complete documentation will be generated for this element (as this indicates that the documentation has already been created manually)
        /// </summary>
        public virtual bool isCustomElement
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// If this is set to false, then this element will not be rendered, but its functions will still be executed
        /// </summary>
        public virtual bool renderElement
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Used to add non-default attributes to this element in the XSD file
        /// It is only necessary to override this if your custom tag handler provides new attributes and you wish for Visual Studio to autocomplete them for you.
        ///
        /// The key is the name of the attribute, and the value is the datatype.
        /// The datatype must be a valid datatype within XmlLayout.xsd or http://www.w3.org/2001/XMLSchema
        /// e.g.
        /// xs:string -> string value (from http://www.w3.org/2001/XMLSchema)
        /// xs:integer -> integer value (from http://www.w3.org/2001/XMLSchema)
        /// xs:float -> float value (from http://www.w3.org/2001/XMLSchema)
        /// xmlLayout:color -> color in hex/rgb/rgba format
        /// xmlLayout:vector2 -> vector 2 in x y format
        /// xmlLayout:floatList -> list of floats e.g. "10 10 10"
        /// </summary>
        public virtual Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// This property will allow you to use built-in attribute groups so that you don't have to redefine the same properties
        /// The built-in attribute groups are:
        /// - simpleAttributes (* provided by most extension types already)
        /// - tooltip (*)
        /// - rectTransform (*)
        /// - layoutElement
        /// - image
        /// - text
        /// - animation
        /// </summary>
        public virtual List<string> attributeGroups
        {
            get
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// This property will allow you to use custom attribute groups so that you don't have to redefine the same properties
        /// multiple times
        /// </summary>
        public virtual List<Type> customAttributeGroups
        {
            get
            {
                return new List<Type>();
            }
        }

        public virtual string extension
        {
            get
            {
                return "base";
            }
        }

        protected virtual bool dontCallHandleDataSourceAttributeAutomatically { get { return false; } }

        /// <summary>
        /// A collection of attribute values, recorded when their values are set for the first time.
        /// The 'original' value (as per the prefab) is recorded prior to setting the value, which can then be used
        /// later on to 'restore' it if the attribute is cleared
        /// </summary>
        private AttributeDictionary _defaultAttributeValues = new AttributeDictionary();
        protected virtual AttributeDictionary defaultAttributeValues
        {
            get
            {
                return _defaultAttributeValues;
            }
        }

        private static int m_uiLayer = -1;
        internal static int uiLayer
        {
            get
            {
                if (m_uiLayer == -1) m_uiLayer = LayerMask.NameToLayer("UI");
                return m_uiLayer;
            }
        }

        /// <summary>
        /// If this is set to true, then this element will allow any attribute to be used without validation
        /// </summary>
        public virtual bool allowAnyAttribute
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Create an instance of this tag's prefab, and make it the current instance being worked on by this tag handler
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual XmlElement GetInstance(RectTransform parent, XmlLayout xmlLayout, string overridePrefabPath = null)
        {
            currentInstanceTransform = Instantiate(parent, overridePrefabPath ?? this.prefabPath);
            var xmlElement = currentInstanceTransform.gameObject.GetComponent<XmlElement>() ?? currentInstanceTransform.gameObject.AddComponent<XmlElement>();

            xmlElement.Initialise(xmlLayout, currentInstanceTransform, this);

            var parentXmlElement = parent.GetComponent<XmlElement>();
            if (parentXmlElement != null)
            {
                parentXmlElement.AddChildElement(xmlElement);
            }

            xmlElement.gameObject.layer = uiLayer;

            return xmlElement;
        }

        /// <summary>
        /// Set the Instance this ElementTagHandler is currently working with.
        /// </summary>
        /// <param name="instanceTransform"></param>
        /// <param name="xmlLayout"></param>
        public virtual void SetInstance(RectTransform instanceTransform, XmlLayout xmlLayout)
        {
            currentInstanceTransform = instanceTransform;
            currentXmlLayoutInstance = xmlLayout;

            var xmlElement = this.currentXmlElement;
            if (instanceTransform != null && xmlElement == null)
            {
                // Normally this won't be necessary, but sometimes we may be applying attribute values to child elements that aren't top-level XmlElements, so just in case
                xmlElement = currentInstanceTransform.gameObject.AddComponent<XmlElement>();
            }

            if (xmlElement != null)
            {
                xmlElement.Initialise(xmlLayout, instanceTransform, this);
            }
        }

        public virtual void SetInstance(XmlElement element)
        {
            SetInstance(element.rectTransform, element.xmlLayoutInstance);
        }

        /// <summary>
        /// Apply the specified attributes to the XmlElement (and its other relevant components)
        /// </summary>
        /// <param name="attributesToApply"></param>
        public virtual void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            if (currentInstanceTransform == null || currentXmlLayoutInstance == null)
            {
                Debug.LogWarning("[XmlLayout][Warning] Please call ElementTagHandler.SetInstance() before using XmlElement.ApplyAttributes()");
                return;
            }

            //var startTime = DateTime.Now;
            attributesToApply = HandleCustomAttributes(attributesToApply);
            var _primaryComponent = primaryComponent;

            // the vast majority of events require the element to block raycasts, so rather than expecting the users to set this value every time,
            // lets set it here
            if (attributesToApply.Any(a => !String.Equals("onValueChanged", a.Key, StringComparison.OrdinalIgnoreCase) && eventAttributeNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)))
            {
                attributesToApply.AddIfKeyNotExists("raycastTarget", "true");
            }
            else if (!String.IsNullOrEmpty(attributesToApply.GetValue("tooltip")))
            {
                attributesToApply.AddIfKeyNotExists("raycastTarget", "true");
            }

            if (attributesToApply.ContainsKey("allowDragging") && attributesToApply["allowDragging"].ToBoolean())
            {
                var dragEventHandler = currentXmlElement.GetComponent<XmlLayoutDragEventHandler>();
                if (dragEventHandler == null) currentXmlElement.gameObject.AddComponent<XmlLayoutDragEventHandler>();
            }

            foreach (var attribute in attributesToApply)
            {
                string name = attribute.Key.ToLower();
                string value = attribute.Value;

                if (eventAttributeNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    // As it happens, events don't work anyway unless they are processed at runtime (which is why we have 'ForceRebuildOnAwake')
                    // so we may as well not process any event attributes at all in edit mode
                    // (this also helps avoid triggering event handlers in edit mode)
                    // Note: we set these at the end of the frame so that any internal event-handlers (such as for MVVM) will take precedence
                    // and be executed first
                    var _xmlLayoutInstance = currentXmlLayoutInstance;
                    var _transform = currentInstanceTransform;

                    if (Application.isPlaying) XmlLayoutTimer.AtEndOfFrame(() =>
                    {
                        if (_transform == null || _xmlLayoutInstance == null) return;

                        SetInstance(_transform, _xmlLayoutInstance);
                        HandleEventAttribute(name, value);
                    }, null, true);

                    continue;
                }

                var propertySetOnComponent = _primaryComponent != null ? SetPropertyValue(_primaryComponent, name, value) : false;

                // if we failed to set the property on the component, perhaps it is a transform value instead
                if (!propertySetOnComponent)
                {
                    var propertySetOnTransform = SetPropertyValue(currentInstanceTransform, name, value);

                    // perhaps it is a layout value
                    if (!propertySetOnTransform)
                    {
                        var propertySetOnLayoutComponent = SetPropertyValue(layoutElement, name, value);

                        // or, perhaps it is an image value
                        if (!propertySetOnLayoutComponent)
                        {
                            // lastly, check the XmlElement
                            var propertySetOnXmlElement = SetPropertyValue(currentXmlElement, name, value);

                            if (!propertySetOnXmlElement)
                            {
                                var _imageComponent = imageComponent;
                                if (_imageComponent != null)
                                {
                                    SetPropertyValue(imageComponent, name, value);
                                }
                            }
                        }
                    }
                }
            }

#if !ENABLE_IL2CPP && MVVM_ENABLED
            if (!dontCallHandleDataSourceAttributeAutomatically && attributesToApply.ContainsKey("vm-dataSource"))
            {
                HandleDataSourceAttribute(attributesToApply["vm-dataSource"]);
            }

            // If this element is associated with a data source, but has no vm-dataSource attribute
            // (perhaps it was removed)
            // then remove the association
            if (!currentXmlElement.attributes.ContainsKey("vm-dataSource"))
            {
                currentXmlLayoutInstance.ElementDataSources.RemoveAll(ed => ed.XmlElement == currentXmlElement);
            }
#endif
        }

        protected bool SetPropertyValue(object o, string propertyName, string value)
        {
            if (o == null) return false;

            var type = o.GetType();

            var fieldInfo = GetComponentXmlField(type, propertyName);

            bool attemptToRecordInitialValue = !this.defaultAttributeValues.ContainsKey(propertyName);
            object initialValue = null;
            bool returnValue = false;


            try
            {
                if (fieldInfo != null)
                {
                    if(attemptToRecordInitialValue) initialValue = fieldInfo.GetValue(o);
                    fieldInfo.SetValue(o, value.ChangeToType(fieldInfo.FieldType, currentXmlLayoutInstance));
                    returnValue = true;
                }
                else
                {
                    var propertyInfo = type.GetProperty(propertyName, XmlLayoutUtilities.BindingFlags);

                    if (propertyInfo != null && propertyInfo.GetSetMethod(false) != null)
                    {
                        if (attemptToRecordInitialValue && propertyInfo.GetGetMethod(false) != null) initialValue = propertyInfo.GetValue(o, XmlLayoutUtilities.BindingFlags, null, null, null);

                        var convertedValue = value.ChangeToType(propertyInfo.PropertyType, currentXmlLayoutInstance);

#if TEXTMESHPRO_PRESENT
                        if ((type == typeof(UnityEngine.UI.InputField) || type == typeof(TMPro.TMP_InputField)) && propertyName == "text")
#else
                        if (type == typeof(UnityEngine.UI.InputField) && propertyName == "text")
#endif
                        {
                            try
                            {
                                // this sometimes triggers an exception when passed a numeric value from a viewmodel.
                                // It shouldn't, and yet it does. I tried delaying until the end of the frame, in case it was an issue with the InputField not being set up
                                // yet, but that still triggered the exception.
                                // It does this even if you set the value without using reflection.
                                propertyInfo.SetValue(o, StringExtensions.DecodeEncodedNonAsciiCharacters((string)convertedValue), null);
                            }
                            catch { }
                        }
                        else
                        {
                            propertyInfo.SetValue(o, convertedValue, null);
                        }

                        returnValue = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("[XmlLayout] " + e.Message + " (propertyName == '" + propertyName + "', value == '" + value + "')");
            }

            if (attemptToRecordInitialValue && initialValue != null)
            {
                var v = initialValue.ConvertToAttributeString();
                defaultAttributeValues.SetValue(propertyName, v);

                // This is pretty hacky; but seeing as this is the only case , for now we'll leave it like this
                if (propertyName == "sprite")
                {
                    defaultAttributeValues.SetValue("image", v);
                }
            }

            return returnValue;
        }

        internal void ApplyEventAttributes()
        {
            var eventAttributesToApply = currentXmlElement.attributes.Where(a => eventAttributeNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)).ToList();

            foreach (var eventAttribute in eventAttributesToApply)
            {
                HandleEventAttribute(eventAttribute.Key.ToLower(), eventAttribute.Value);
            }
        }

        protected struct EventData
        {
            public string methodName;
            public string value;
            public List<string> parameters;
        }

        protected EventData GetEventValueData(string eventValue)
        {
            var eventData = eventValue.Trim(new Char[] { ')', ';' })
                                      .Split(',', '(');
            string value = null;
            List<string> parameters = new List<string>();
            if (eventData.Count() > 1)
            {
                value = eventData[1];

                for (var i = 1; i < eventData.Length; ++i)
                {
                    parameters.Add(eventData[i]);
                }
            }

            return new EventData { methodName = eventData[0], value = value, parameters =  parameters };
        }

        protected virtual void HandleEventAttribute(string eventName, string eventValue)
        {
            var layout = currentXmlLayoutInstance;

            if (layout.XmlLayoutController == null)
            {
                Debug.LogError("[XmlLayout] Attempted to process an event attribute for an XmlLayout with no XmlLayoutController attached.");
                return;
            }

            var eventData = GetEventValueData(eventValue);

            if (eventName.Equals("OnElementDropped", StringComparison.OrdinalIgnoreCase))
            {
                HandleOnDroppedEventAttribute(eventData.methodName);
                return;
            }

            var transform = currentInstanceTransform;
            var _component = primaryComponent;


            var xmlElement = currentInstanceTransform.GetComponent<XmlElement>();

            if (mouseEventNames.Contains(eventName, StringComparer.OrdinalIgnoreCase))
            {
                Action<PointerEventData> action = (e) =>
                {
                    if (IsInteractable(_component))
                    {
                        string value = eventData.value;
                        if (eventData.value == "pointerId")
                        {
                            value = e.pointerId.ToString();
                        }

                        layout.XmlLayoutController.ReceiveMessage(eventData.methodName, value, transform, eventData.parameters, e.pointerId);
                    }
                };

                switch (eventName.ToLower())
                {
                    case "onclick":
                        xmlElement.AddOnClickEvent(action, true);
                        break;
                    case "onmouseenter":
                        xmlElement.AddOnMouseEnterEvent(action, true);
                        break;
                    case "onmouseexit":
                        xmlElement.AddOnMouseExitEvent(action, true);
                        break;
                    case "onmousedown":
                        xmlElement.AddOnMouseDownEvent(action, true);
                        break;
                    case "onmouseup":
                        xmlElement.AddOnMouseUpEvent(action, true);
                        break;
                }
            }
            else
            {
                Action action = () =>
                {
                    if (IsInteractable(_component))
                    {
                        layout.XmlLayoutController.ReceiveMessage(eventData.methodName, eventData.value, transform, eventData.parameters);
                    }
                };

                switch (eventName.ToLower())
                {
                    case "ondrag":
                        xmlElement.AddOnDragEvent(action, true);
                        break;
                    case "onbegindrag":
                        xmlElement.AddOnBeginDragEvent(action, true);
                        break;
                    case "onenddrag":
                        xmlElement.AddOnEndDragEvent(action, true);
                        break;
                    case "onsubmit":
                        xmlElement.AddOnSubmitEvent(action, true);
                        break;
                    case "onshow":
                        xmlElement.AddOnShowEvent(action, true);
                        break;
                    case "onhide":
                        xmlElement.AddOnHideEvent(action, true);
                        break;

                    default:
                        Debug.LogWarning("[XmlLayout] Unknown event type: '" + eventName + "'");
                        return;
                }

            }

        }

        private bool IsInteractable(MonoBehaviour component)
        {
            System.Reflection.PropertyInfo interactablePropertyInfo = null;

            if (component != null)
            {
                var type = component.GetType();
                interactablePropertyInfo = type.GetProperty("interactable");

                if (interactablePropertyInfo != null)
                {
                    return (bool)interactablePropertyInfo.GetValue(component, null);
                }
            }

            return true;
        }

        protected void HandleOnDroppedEventAttribute(string value)
        {
            var xmlElement = currentInstanceTransform.GetComponent<XmlElement>();
            var layout = currentXmlLayoutInstance;

            Action<XmlElement, XmlElement> action = (item, droppedOn) =>
            {
                layout.XmlLayoutController.ReceiveElementDroppedMessage(value, item, droppedOn);
            };

            xmlElement.AddOnElementDroppedEvent(action);
        }

        /// <summary>
        /// Convert custom attributes (that aren't found via reflection, e.g. width/height) into useable values
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        protected AttributeDictionary HandleCustomAttributes(AttributeDictionary attributes)
        {
            var elementName = XmlLayoutUtilities.GetTagName(GetType());
            //var elementName = this.GetType().Name.Replace("TagHandler", String.Empty);
            var customAttributes = attributes.Where(k => XmlLayoutUtilities.IsCustomAttribute(k.Key)).ToList();

            foreach (var attribute in customAttributes)
            {
                var customAttribute = XmlLayoutUtilities.GetCustomAttribute(attribute.Key);

                if (customAttribute.RestrictToPermittedElementsOnly)
                {
                    if (!customAttribute.PermittedElements.Contains(elementName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (customAttribute.UsesConvertMethod)
                {
                    attributes = XmlLayoutUtilities.MergeAttributes(
                                            attributes,
                                            customAttribute.Convert(attribute.Value, attributes.AsReadOnly(), this.currentXmlElement));
                }

                if (customAttribute.UsesApplyMethod)
                {
                    customAttribute.Apply(currentXmlElement, attribute.Value, attributes.AsReadOnly());
                }

                if (customAttribute.UsesApplyMethod && !defaultAttributeValues.ContainsKey(attribute.Key)) defaultAttributeValues.Add(attribute.Key, customAttribute.DefaultValue);

                if (!customAttribute.KeepOriginalTag)
                {
                    attributes.Remove(attribute.Key);
                }
            }

            return attributes;
        }

        protected RectTransform Instantiate(RectTransform parent, string name = "")
        {
            var prefab = XmlLayoutUtilities.LoadResource<GameObject>(name);
            GameObject gameObject = null;
            RectTransform transform = null;

            if (prefab != null)
            {
                gameObject = GameObject.Instantiate<GameObject>(prefab);
                transform = gameObject.GetComponent<RectTransform>();

                transform.SetParent(parent);

                FixInstanceTransform(prefab.transform as RectTransform, transform);
            }
            else
            {
                if (!String.IsNullOrEmpty(name))
                {
                    Debug.Log("Warning: prefab '" + name + "' not found.");
                }
                gameObject = new GameObject(name);
            }

            if (transform == null)
            {
                transform = gameObject.AddComponent<RectTransform>();
            }

            if (name != null && name.Contains("/") && !name.EndsWith("/"))
            {
                name = name.Substring(name.LastIndexOf("/") + 1);
            }

            gameObject.name = name ?? "Xml Element";

            if (transform.parent != parent)
            {
                transform.SetParent(parent);
            }

            return transform;
        }

        /// <summary>
        /// Correct the position, rotation, scale, etc. of the specified transform.
        /// Unity tends to seemingly randomize some of these values on newly-instantiated objects;
        /// this method corrects that.
        /// </summary>
        /// <param name="baseTransform"></param>
        /// <param name="instanceTransform"></param>
        protected static void FixInstanceTransform(RectTransform baseTransform, RectTransform instanceTransform)
        {
            instanceTransform.localPosition = baseTransform.localPosition;
            instanceTransform.position = baseTransform.position;
            //instanceTransform.rotation = baseTransform.rotation;
            instanceTransform.rotation = new Quaternion();

            instanceTransform.localScale = baseTransform.localScale;
            instanceTransform.anchoredPosition = baseTransform.anchoredPosition;
            instanceTransform.sizeDelta = baseTransform.sizeDelta;

            instanceTransform.position = new Vector3(instanceTransform.position.x, instanceTransform.position.y, 0);
            instanceTransform.anchoredPosition3D = new Vector3(baseTransform.anchoredPosition3D.x, baseTransform.anchoredPosition3D.y, 0);
        }

        /// <summary>
        /// Called when the tag is opened
        /// </summary>
        public virtual void Open(AttributeDictionary elementAttributes)
        {
        }

        /// <summary>
        /// Called when the tag is closed
        /// </summary>
        public virtual void Close()
        {
        }

        public virtual bool UseParseChildElements { get { return false; } }

        /// <summary>
        /// If this function returns true, then XmlLayout will consider this element to be completely parsed and will not try to parse the child nodes normally.
        /// Only overriden for a select few element tag handlers, e.g. Dropdown.
        /// NOTE: As of XmlLayout v1.62, this will only be called if 'UseParseChildElements' is true for this tag handler.
        /// - This method now returns void
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public virtual void ParseChildElements(System.Xml.XmlNode xmlNode)
        {
        }

        /// <summary>
        /// Remove the Current element from the scene.
        /// </summary>
        public void RemoveElement()
        {
            if (currentXmlElement.parentElement != null) currentXmlElement.parentElement.RemoveChildElement(currentXmlElement);

            if (Application.isPlaying)
            {
                GameObject.Destroy(currentXmlElement.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(currentXmlElement.gameObject);
            }
        }

        /// <summary>
        /// Set the value of the current element.
        /// (Used by MVVM)
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetValue(string newValue, bool fireEventHandlers = true)
        {
            // default behaviour: set text attribute
            ApplyAttributes(
                new AttributeDictionary()
                {
                    {"text", newValue}
                });
        }

        public virtual void ClassChanged()
        {
            if (currentXmlLayoutInstance.defaultAttributeValues.ContainsKey(this.tagType))
            {
                var defaultAttributesMerged = new AttributeDictionary();

                var elementAttributes = new AttributeDictionary(currentXmlElement.elementAttributes.ToDictionary(k => k, v => currentXmlElement.GetAttribute(v)));

                defaultAttributesMerged = currentXmlLayoutInstance.MergeDefaultAttributesWithElementAttributes(currentXmlElement, this.tagType, elementAttributes);

                // remove any class attributes that have been defined directly on the element
                defaultAttributesMerged = new AttributeDictionary(defaultAttributesMerged.Where(a => !currentXmlElement.elementAttributes.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value));

                // merge in the updated class attributes and apply them
                //currentXmlElement.attributes = XmlLayoutUtilities.MergeAttributes(defaultAttributesMerged, elementAttributes);
                currentXmlElement.ApplyAttributes(defaultAttributesMerged);
            }
        }

        public string GetDefaultValueForAttribute(string attribute)
        {
            return defaultAttributeValues.ContainsKey(attribute) ? defaultAttributeValues[attribute] : String.Empty;
        }

        protected void MatchParentDimensions()
        {
            var parent = currentInstanceTransform.parent as RectTransform;

            currentInstanceTransform.localPosition = Vector3.zero;
            currentInstanceTransform.anchoredPosition3D = Vector3.zero;
            currentInstanceTransform.anchorMin = Vector2.zero;
            currentInstanceTransform.anchorMax = Vector2.one;
            currentInstanceTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.rect.width);
            currentInstanceTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parent.rect.height);
            currentXmlElement.currentOffset = Vector2.zero;
        }

        /// <summary>
        /// Returns true if the current xml element has the specified attribute, or if it is contained in the list of attributes that are being applied
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributesToApply"></param>
        /// <returns></returns>
        protected bool ElementHasAttribute(string attributeName, AttributeDictionary attributesToApply, XmlElement element = null)
        {
            if(element == null) element = currentXmlElement;

            return attributesToApply.ContainsKey(attributeName) || element.HasAttribute(attributeName);
        }
    }
}
