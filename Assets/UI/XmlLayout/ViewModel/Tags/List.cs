#if !ENABLE_IL2CPP && MVVM_ENABLED
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.Events;

namespace UI.Xml.Tags
{
    /// <summary>
    /// An element which uses a user-provided template to create a list of elements
    /// (which are sourced from the View Model)
    /// </summary>
    public class ListTagHandler : ElementTagHandler, IObservableListTagHandler
    {
        /// <summary>
        /// A collection of List elements present in the scene
        /// (used to quickly locate them for ViewModel updates)
        /// </summary>
        internal Dictionary<string, XmlLayoutList> ListElements = new Dictionary<string, XmlLayoutList>();

        // Don't use a prefab, just use an empty gameobject
        public override string prefabPath { get { return null; } }

        // Remove the gameobject for this element once processing is complete
        public override bool renderElement { get { return false; } }

        // Don't generate documentation for this element (it has been added manually)
        public override bool isCustomElement { get { return false; } }

        // Don't use any default attributes/etc.
        public override string extension { get { return "blank"; } }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"vm-dataSource", "xs:string"},
                    {"itemShowAnimation", "None,Grow,Grow_Vertical,Grow_Horizontal,FadeIn,SlideIn_Left,SlideIn_Right,SlideIn_Top,SlideIn_Bottom"}
                };
            }
        }

        // Add children to the parent of this element (the actual List element will be removed once processing is complete)
        public override RectTransform transformToAddChildrenTo
        {
            get
            {
                return currentInstanceTransform != null ? currentInstanceTransform.parent as RectTransform : null;
            }
        }

        protected XmlLayoutList currentListElement = null;

        public override void SetInstance(RectTransform instanceTransform, XmlLayout xmlLayout)
        {
            base.SetInstance(instanceTransform, xmlLayout);

            if(instanceTransform != null) currentListElement = instanceTransform.GetComponent<XmlLayoutList>();
        }

        public override bool UseParseChildElements { get { return true; } }

        public override void ParseChildElements(System.Xml.XmlNode xmlNode)
        {
            if (String.IsNullOrEmpty(currentXmlElement.DataSource))
            {
                // no data source; no rendering to do here
                // I've decided not to log anything here, as this could be desired behaviour (a data source could be set later, for example)
                return;
            }

            var dataSource = currentXmlElement.DataSource;
            var xmlLayoutController = currentXmlLayoutInstance.XmlLayoutController;
            if (xmlLayoutController == null) return;

            var viewModelProperty = xmlLayoutController.GetType().GetProperty("viewModel");
            if (viewModelProperty == null)
            {
                Debug.LogWarning("[XmlLayout] Warning: Useage of the <List> element requires the XmlLayoutController to have a view model type defined.");
                return;
            }

            var viewModel = viewModelProperty.GetValue(xmlLayoutController, null);
            var listProperty = viewModel.GetType().GetProperty(dataSource);
            var listField = viewModel.GetType().GetField(dataSource);

            IList list = null;
            if (listProperty != null)
            {
                if (!(listProperty.PropertyType.IsGenericType && listProperty.PropertyType.GetGenericTypeDefinition() == typeof(ObservableList<>)))
                {
                    Debug.LogWarning("[XmlLayout] Warning: Usage of the <List> element requires a property with a type of ObservableList.");
                    return;
                }

                list = (IList)listProperty.GetValue(viewModel, XmlLayoutUtilities.BindingFlags, null, null, null);
            }
            else if (listField != null)
            {
                if (!(listField.FieldType.IsGenericType && listField.FieldType.GetGenericTypeDefinition() == typeof(ObservableList<>)))
                {
                    Debug.LogWarning("[XmlLayout] Warning: Usage of the <List> element requires a property with a type of ObservableList.");
                    return;
                }

                list = (IList)listField.GetValue(viewModel);
            }
            else
            {
                Debug.LogWarning("[XmlLayout] Warning: View Model does not contain a field or property for data source '" + dataSource + "'.");
                return;
            }

            var observableList = (IObservableList)list;

            if (list == null || observableList == null)
            {
                // no data yet
                return;
            }

            var parent = transformToAddChildrenTo.GetComponent<XmlElement>();

            if (!parent.attributes.ContainsKey("id"))
            {
                XmlLayoutTimer.AtEndOfFrame(() => parent.SetAndApplyAttribute("id", observableList.guid), parent, true);
            }

            var itemTemplate = GetItemTemplate(xmlNode.InnerXml);

            var xmlLayoutListComponent = parent.GetComponent<XmlLayoutList>();
            if (xmlLayoutListComponent == null) xmlLayoutListComponent = parent.gameObject.AddComponent<XmlLayoutList>();
            xmlLayoutListComponent.itemTemplate = itemTemplate;
            xmlLayoutListComponent.DataSource = dataSource;
            xmlLayoutListComponent.baseSiblingIndex = currentXmlElement.transform.GetSiblingIndex();
            xmlLayoutListComponent.list = observableList;
            xmlLayoutListComponent.isCalculatedList = !listProperty.IsAutoProperty();

            xmlLayoutListComponent.itemAnimationDuration = currentXmlElement.attributes.ContainsKey("itemAnimationDuration") ? currentXmlElement.attributes.GetValue<float>("itemAnimationDuration") : 0.25f;
            xmlLayoutListComponent.itemShowAnimation = currentXmlElement.attributes.ContainsKey("itemShowAnimation") ? currentXmlElement.attributes.GetValue<string>("itemShowAnimation") : "None";
            xmlLayoutListComponent.itemHideAnimation = currentXmlElement.attributes.ContainsKey("itemHideAnimation") ? currentXmlElement.attributes.GetValue<string>("itemHideAnimation") : "None";

            currentListElement = xmlLayoutListComponent;

            if (ListElements.ContainsKey(observableList.guid))
            {
                ListElements[observableList.guid] = xmlLayoutListComponent;
            }
            else
            {
                ListElements.Add(observableList.guid, xmlLayoutListComponent);
            }

            // Render the list items as per the view model
            for (var x = 0; x < list.Count; x++)
            {
                RenderListItem(list[x], dataSource, itemTemplate, observableList);
            }
        }

        internal void ProcessCalculatedListUpdate(IObservableList updatedList)
        {
            var originalList = currentListElement.list;

            // update to the latest 'version' of this list
            currentListElement.list = updatedList;

            //var listItems = updatedList.GetItems();

            var count = Math.Max(originalList.Count, updatedList.Count);

            for (var x = 0; x < count; x++)
            {
                var itemElement = (x < currentListElement.listItems.Count) ? currentListElement.listItems[x] : null;

                if (itemElement != null)
                {
                    if (x < updatedList.Count)
                    {
                        ApplyViewModelData(itemElement.xmlElement, updatedList[x], currentListElement.DataSource, currentListElement.itemTemplate, updatedList);
                    }
                    else if (x >= updatedList.Count)
                    {
                        RemoveListItemByIndexFromCurrentList(originalList, x, currentListElement.DataSource);
                    }
                }
                else
                {
                    if (x < updatedList.Count)
                    {
                        RenderListItem(updatedList[x], currentListElement.DataSource, currentListElement.itemTemplate, updatedList);
                    }
                }
            }
        }

        private void RemoveListItemByIndexFromCurrentList(IObservableList list, int index, string listName)
        {
            var itemElement = currentListElement.listItems[index];
            _RemoveListItem(currentListElement, itemElement);
        }

        public void RemoveListItem(IObservableList list, object item, string listName)
        {
            var listElement = ListElements[list.guid];

            var itemGuid = list.GetGUID(item);
            var itemElement = listElement.listItems.FirstOrDefault(f => f.guid == itemGuid);

            if (itemElement != null)
            {
                itemElement.xmlElement.Hide(false, () => _RemoveListItem(listElement, itemElement));
            }
        }

        private void _RemoveListItem(XmlLayoutList list, XmlLayoutListItem item)
        {
            if (item.xmlElement != null)
            {
                list.listItems.Remove(item);
                list.listElement.RemoveChildElement(item.xmlElement);
            }

            if (Application.isPlaying)
            {
                GameObject.Destroy(item.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(item.gameObject);
            }

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(list.rectTransform);
            }, list);
        }

        /// <summary>
        /// Create a template from the provided Xml and return it as an XmlElement
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        XmlElement GetItemTemplate(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Give the template an id so we can retrieve it
            var id = "ListItemTemplate-" + Guid.NewGuid().ToString();

            doc.DocumentElement.SetAttribute("id", id);

            // make the template inactive so that it is not visible
            doc.DocumentElement.SetAttribute("active", "false");

            // parse the xml
            var reader = XmlReader.Create(new System.IO.StringReader(doc.DocumentElement.OuterXml));
            reader.Read();

            currentXmlLayoutInstance.ParseNode(reader, transformToAddChildrenTo, null, true);

            // locate the new element by id and return it
            return currentXmlLayoutInstance.GetElementById(id);
        }

        /// <summary>
        /// Render a new list item element
        /// </summary>
        /// <param name="itemData"></param>
        /// <param name="dataSource"></param>
        /// <param name="itemTemplate"></param>
        /// <param name="list"></param>
        internal void RenderListItem(object itemData, string dataSource, XmlElement itemTemplate, IObservableList list)
        {
            // Create the new item based on the item template for this list
            var element = (XmlElement)GameObject.Instantiate(itemTemplate);
            var parent = currentListElement.listElement;

            // Add the new item to the list container
            parent.AddChildElement(element);

            // The template will be inactive by default, we need to make it active
            element.SetAttribute("active", "true");

            // Get/Add the List Item component and add it to the element
            var listItemComponent = element.gameObject.GetComponent<XmlLayoutListItem>() ?? element.gameObject.AddComponent<XmlLayoutListItem>();
            listItemComponent.guid = list.GetGUID(itemData);

            // Add this list item to the list's item collection
            var index = list.IndexOf(itemData);
            currentListElement.listItems.Insert(index, listItemComponent);

            // Load the data from 'itemData' and apply it to our new list element
            ApplyViewModelData(element, itemData, dataSource, itemTemplate, list, null, true, true);

            // apply our attributes (especially necessary for things like event handlers and the like)
            element.Initialise(currentXmlLayoutInstance, element.rectTransform, element.tagHandler);
            element.ApplyAttributes();

            element.AnimationDuration = currentListElement.itemAnimationDuration;
            element.ShowAnimation = currentListElement.itemShowAnimation;
            element.HideAnimation = currentListElement.itemHideAnimation;

            if (element.ShowAnimation != "None") element.Show();

            // Rebuild the layout at the end of the frame
            XmlLayoutTimer.AtEndOfFrame(() => LayoutRebuilder.MarkLayoutForRebuild(element.rectTransform), element);

#if UNITY_5_4
            // Due to differences in how 5.4 and 5.5 handle layout rebuilds, we sometimes have to manually notify child TableLayouts to update
            XmlLayoutTimer.DelayedCall(0.05f, () =>
                {
                    var tableLayouts = element.GetComponentsInChildren<UI.Tables.TableLayout>();
                    foreach (var tableLayout in tableLayouts)
                    {
                        tableLayout.UpdateLayout();
                    }
                }, element);
#endif
        }

        /// <summary>
        /// Load data from 'itemData' and apply it to 'element'
        /// (Also iterates recursively through all child elements)
        /// </summary>
        /// <param name="element">Element to apply the data to</param>
        /// <param name="itemData">Object containing the data to use</param>
        /// <param name="dataSource">Required to locate the relevant properties e.g. {dataSource.someProperty}</param>
        /// <param name="elementTemplate">Used to determine which attribute(s) to replace with data from 'itemData'</param>
        internal void ApplyViewModelData(   XmlElement element,
                                            object itemData,
                                            string dataSource,
                                            XmlElement elementTemplate,
                                            IObservableList list,
                                            string changedField = null,
                                            bool isTopLevel = true,
                                            bool isFirstCall = false)
        {
            // Iterate through the children first
            for(var x = 0; x < element.childElements.Count; x++)
            {
                var child = element.childElements[x];

                ApplyViewModelData(child, itemData, dataSource, elementTemplate.childElements[x], list, changedField, false, isFirstCall);
            }

            List<ListItemAttributeMatch> attributesMatched = new List<ListItemAttributeMatch>();

            // Get a list of potential attributes to replace with data
            var attributesToCheck = elementTemplate.attributes.Where(a => a.Value.StartsWith("{") && a.Value.EndsWith("}"))
                                                              .ToDictionary(k => k.Key, v => v.Value.Replace("{", string.Empty).Replace("}", string.Empty));

            if (attributesToCheck.Count > 0)
            {
                // Get a list of all fields and properties provided by the ViewModel object
                var members = itemData.GetType()
                                      .GetMembers()
                                      .Where(m => {
                                          return changedField == null                                           // If no specific changed field was provided, pull all members
                                              || m.Name == changedField                                         // otherwise, pull the changed field
                                              || (m is PropertyInfo && !((PropertyInfo)m).IsAutoProperty());    // as well as all non-auto generated properties (e.g. properties other than { get; set; })
                                      })
                                      .Where(m => m is PropertyInfo || m is FieldInfo)
                                      .ToList();

                // intersect the fields/attributes
                var fieldsToApply = members.Where(m => attributesToCheck.Values.ToList().Any(a => a.StripChars('?') == (dataSource + "." + m.Name))).ToList();

                // Get the data from each field/property, and set the attribute on the element
                foreach (var field in fieldsToApply)
                {
                    var attribute = attributesToCheck.FirstOrDefault(a => a.Value.StripChars('?') == dataSource + "." + field.Name);
                    var value = ConversionExtensions.ToString(((field is PropertyInfo ? ((PropertyInfo)field).GetValue(itemData, null) : ((FieldInfo)field).GetValue(itemData)) ?? string.Empty));

                    element.SetAttribute(attribute.Key, value);
                    attributesMatched.Add(new ListItemAttributeMatch
                    {
                        attribute = attribute.Key,
                        field = field.Name,
                        bindingType = attribute.Value.StartsWith("?") ? ViewModelBindingType.OneWay : ViewModelBindingType.TwoWay
                    });
                }
            }


            // Apply the attributes (if necessary)
            if (isFirstCall || attributesMatched.Count > 0)
            {
                element.ApplyAttributes(element.attributes.Clone());
            }

            var listItemComponent = element.GetComponent<XmlLayoutListItem>();

            if (listItemComponent != null && currentListElement != null)
            {
                if(isTopLevel) HandleListItemPositioning(listItemComponent);
            }

            if (attributesMatched.Count > 0) HandleTwoWayBinding(element, dataSource, list, itemData, attributesMatched);
        }

        void HandleTwoWayBinding(XmlElement element, string dataSource, IObservableList list, object itemData, List<ListItemAttributeMatch> attributes)
        {
            if (element.HasAttribute("__twoWayBindingSetupComplete")) return;

            var tagHandler = element.tagHandler;

            if (tagHandler is IHasXmlFormValue)
            {
                var attribute = attributes.FirstOrDefault(a => a.bindingType == ViewModelBindingType.TwoWay
                                                           && (a.attribute.Equals("value", StringComparison.OrdinalIgnoreCase)
                                                            || a.attribute.Equals("text", StringComparison.OrdinalIgnoreCase)
                                                            || a.attribute.Equals("ison", StringComparison.OrdinalIgnoreCase)));

                if (attribute != null)
                {
                    var memberName = attribute.field;

                    tagHandler.SetInstance(element.rectTransform, currentXmlLayoutInstance);

                    if (tagHandler.primaryComponent == null) return;

                    var componentType = tagHandler.primaryComponent.GetType();
                    var onValueChangedMemberInfo = componentType.GetMember("onValueChanged").FirstOrDefault();


                    if (onValueChangedMemberInfo != null)
                    {
                        var onValueChangedListener = onValueChangedMemberInfo.GetMemberValue(tagHandler.primaryComponent);
                        var addListenerMethod = onValueChangedListener.GetType().GetMethod("AddListener");

                        var eventType = addListenerMethod.GetParameters()[0].ParameterType;
                        var parameterType = eventType.GetGenericArguments()[0];

                        var controller = (XmlLayoutControllerMVVM)currentXmlLayoutInstance.XmlLayoutController;

                        // I tried to implement this more generically with reflection but had no luck. I'm sure there is a way to do it...
                        // I'll try again at a later date.
                        if (parameterType == typeof(float))
                        {
                            ((UnityEvent<float>)onValueChangedListener).AddListener((v) => controller.SetViewModelListItemValue(dataSource, list.IndexOf(itemData), memberName, v, true));
                        }
                        else if (parameterType == typeof(int))
                        {
                            ((UnityEvent<int>)onValueChangedListener).AddListener((v) => controller.SetViewModelListItemValue(dataSource, list.IndexOf(itemData), memberName, v, true));
                        }
                        else if (parameterType == typeof(string))
                        {
                            ((UnityEvent<string>)onValueChangedListener).AddListener((v) => controller.SetViewModelListItemValue(dataSource, list.IndexOf(itemData), memberName, v, true));
                        }
                        else if (parameterType == typeof(bool))
                        {
                            ((UnityEvent<bool>)onValueChangedListener).AddListener((v) => controller.SetViewModelListItemValue(dataSource, list.IndexOf(itemData), memberName, v, true));
                        }

                        // Mark this element as having its binding setup complete (so that we don't repeat this)
                        element.SetAttribute("__twoWayBindingSetupComplete", string.Empty);
                    }
                }
            }
        }

        void HandleListItemPositioning(XmlLayoutListItem listItem)
        {
            if (currentListElement.listItems.Count > 0 && currentListElement.listItems.IndexOf(listItem) != 0)
            {
                var firstlistItem = currentListElement.listItems.FirstOrDefault();

                if (firstlistItem != null)
                {
                    var thisListItemIndex = currentListElement.listItems.IndexOf(listItem);

                    if (thisListItemIndex == -1) return;

                    var desiredSiblingIndex = firstlistItem.transform.GetSiblingIndex() + thisListItemIndex;

                    listItem.xmlElement.rectTransform.SetSiblingIndex(desiredSiblingIndex);
                }
            }
            else
            {
                listItem.xmlElement.rectTransform.SetSiblingIndex(currentListElement.baseSiblingIndex);
            }
        }

        public void UpdateListItem(IObservableList list, int index, object item, string listName, string changedField = null)
        {
            var listElement = ListElements[list.guid];
            var itemGuid = list.GetGUID(item);
            var itemElement = listElement.listItems.FirstOrDefault(f => f.guid == itemGuid);

            SetInstance(listElement.rectTransform, listElement.listElement.xmlLayoutInstance);

            //this.SetInstance(itemElement.xmlElement.rectTransform, itemElement.xmlElement.xmlLayoutInstance);
            this.ApplyViewModelData(itemElement.xmlElement, item, listElement.DataSource, listElement.itemTemplate, list, changedField);

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(listElement.rectTransform);
            }, listElement);
        }

        public void AddListItem(IObservableList list, object item, string listName)
        {
            var listElement = ListElements[list.guid];
            this.SetInstance(listElement.rectTransform, listElement.listElement.xmlLayoutInstance);

            RenderListItem(item, listElement.DataSource, listElement.itemTemplate, list);

            var row = listElement.GetComponent<UI.Tables.TableRow>();
            if (row != null) row.NotifyTableRowPropertiesChanged();

            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(listElement.rectTransform);
            }, listElement);
        }


        public bool IsHandlingList(IObservableList list)
        {
            return ListElements.ContainsKey(list.guid);
        }

        private class ListItemAttributeMatch
        {
            public string attribute;
            public string field;
            public ViewModelBindingType bindingType = ViewModelBindingType.TwoWay;
        }
    }
}
#endif
