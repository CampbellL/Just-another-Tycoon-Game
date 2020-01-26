#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UI.Xml.Tags;

namespace UI.Xml
{
    public abstract class XmlLayoutControllerMVVM : XmlLayoutController
    {
        public abstract void SetViewModelValue(string memberName, object newValue, bool fromTwoWayBinding = false);
        public abstract Type GetViewModelMemberDataType(string memberName);
        public abstract void SetViewModelListItemValue(string listName, int index, string memberName, object newValue, bool fromTwoWayBinding = false);
    }

    public class XmlLayoutController<T> : XmlLayoutControllerMVVM
        where T : XmlLayoutViewModel, new()
    {
        private bool _viewModelUpdatePending = false;
        private bool _viewModelPrepopulated = false;
        public bool listenForViewModelChanges = true;

        protected T _viewModel;

        /// <summary>
        /// The ViewModel used by this controller.
        /// </summary>
        public T viewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = new T();
                    InitialiseViewModelProxy(_viewModel);

                    XmlLayoutTimer.AtEndOfFrame(TriggerPrepopulateViewModelData, this, true);
                }

                return _viewModel;
            }
            set
            {
                _viewModel = value;
                InitialiseViewModelProxy(_viewModel);

                ViewModelUpdated();
            }
        }

        private void InitialiseViewModelProxy(T viewModel)
        {
            _viewModel = XmlLayoutViewModel<T>.Create(_viewModel);
            _viewModel.Initialise(this);
        }

        internal override string ProcessViewModel(string xml)
        {
            if (viewModel == null) return xml;

            if (!_viewModelPrepopulated) TriggerPrepopulateViewModelData();

            var properties = GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(viewModel, XmlLayoutUtilities.BindingFlags, null, null, null);
                xml = xml.Replace("{" + property.Name + "}", value != null ? value.ToString() : string.Empty);
            }

            return xml;
        }

        private List<PropertyInfo> _properties = null;

        private List<PropertyInfo> GetProperties()
        {
            if(_properties == null) _properties = typeof(T).GetProperties().ToList();

            return _properties;
        }

        /// <summary>
        /// This method will be called whenever a ViewModel member has been changed.
        /// </summary>
        /// <param name="memberName"></param>
        public virtual void OnTwoWayBoundViewModelMemberChanged(string memberName)
        {
            ViewModelMemberChanged(memberName);
        }

        /// <summary>
        /// This method will be called whenever an item contained within a ViewModel list has been changed.
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="index"></param>
        /// <param name="itemProperty"></param>
        public virtual void OnTwoWayBoundViewModelListItemMemberChanged(string listName, int index, string itemProperty = null)
        {
        }

        public override void ViewModelMemberChanged(string memberName)
        {
            if (!listenForViewModelChanges) return;

            // if this member is inline, then a complete rebuild is necessary
            if (xmlLayout.Xml.Contains("{" + memberName + "}"))
            {
                ViewModelUpdated();
                return;
            }

            // otherwise, look for the element(s) using the data source string
            var elements = xmlLayout.GetElementsForDataSource(memberName, memberName);

            if (elements.Count > 0)
            {
                object value = MVVMUtilities.GetNestedMemberValue(typeof(T), memberName, viewModel);

                if (value is IObservableList)
                {
                    elements.ForEach(e => e.SetListData((IObservableList)value));
                }
                else
                {
                    elements.ForEach(e => e.SetValue(value != null ? value.ToString() : null, false));
                }
            }

            return;
        }

        internal void UpdateDataSourcePropertyValue(string propertyName)
        {
            if (!listenForViewModelChanges) return;
            if (String.IsNullOrEmpty(propertyName)) return;

            var elements = xmlLayout.GetElementsForDataSource(propertyName, propertyName);

            if (elements.Count > 0)
            {
                object value = MVVMUtilities.GetNestedMemberValue(typeof(T), propertyName, viewModel);

                if (value is IObservableList)
                {
                    elements.ForEach(e => e.SetListData((IObservableList)value));
                }
                else
                {
                    elements.ForEach(e => e.SetValue(value != null ? value.ToString() : null, false));
                }
            }
        }

        internal override void ViewModelUpdated(bool triggerLayoutRebuild = true)
        {
            if (!listenForViewModelChanges) return;
            if (_viewModelUpdatePending) return;

            _viewModelUpdatePending = true;

            // We wait until the end of the frame so as not to rebuild multiple times if multiple values change in one go
            XmlLayoutTimer.AtEndOfFrame(() =>
            {
                if (triggerLayoutRebuild) xmlLayout.RebuildLayout(true);

                if (xmlLayout.ElementDataSources.Count > 0)
                {
                    xmlLayout.ElementDataSources.Where(e => e is Tags.XmlLayoutDropdownDataSource)
                                                .Select(e => ((Tags.XmlLayoutDropdownDataSource)e).OptionsDataSource)
                                                .Distinct()
                                                .ToList()
                                                .ForEach(el => UpdateDataSourcePropertyValue(el));

                    xmlLayout.ElementDataSources.Select(e => e.DataSource)
                                                .Distinct()
                                                .ToList()
                                                .ForEach(ed => UpdateDataSourcePropertyValue(ed));
                }

                _viewModelUpdatePending = false;

                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(xmlLayout.transform as RectTransform);
                },
                this);

            }, this);
        }


        private void TriggerPrepopulateViewModelData()
        {
            // This should only ever execute once
            if (_viewModelPrepopulated) return;

            var temp = listenForViewModelChanges;

            listenForViewModelChanges = false;
            PrepopulateViewModelData();
            //viewModel.PopulateCalculatedListGUIDs();
            listenForViewModelChanges = temp;

            _viewModelPrepopulated = true;
        }
        /// <summary>
        /// Override this method to populate your view model in code
        /// (if you wish to do so)
        /// </summary>
        protected virtual void PrepopulateViewModelData()
        {
        }

        public override void SetViewModelValue(string memberName, object newValue, bool fromTwoWayBinding = false)
        {
            viewModel.SetValue(memberName, newValue);

            if (fromTwoWayBinding) OnTwoWayBoundViewModelMemberChanged(memberName);
        }

        public override void SetViewModelListItemValue(string listName, int index, string memberName, object newValue, bool fromTwoWayBinding = false)
        {
            viewModel.SetListItemValue(listName, index, memberName, newValue);

            if (fromTwoWayBinding) OnTwoWayBoundViewModelListItemMemberChanged(listName, index, memberName);
        }

        /// <summary>
        /// Get the data type of a particular viewModel member (field or property)
        /// This is useful when determining what type of data for certain tags to provide when updating the viewModel (e.g. DropDown can provide an int or a string)
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public override Type GetViewModelMemberDataType(string memberName)
        {
            var viewModelType = viewModel.GetType();

            var memberInfo = viewModelType.GetMember(memberName).FirstOrDefault();

            if (memberInfo != null)
            {
                return memberInfo.GetMemberDataType();
            }

            return null;
        }

        /// <summary>
        /// ReceiveMessage() with special MVVM handling for List elements
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        public override void ReceiveMessage(string methodName, string value, RectTransform source = null, List<string> parameters = null, int pointerId = 0)
        {
            if (SuppressEventHandling) return;

            if (value != null && value.StartsWith("{") && value.EndsWith("}") && value.Contains('.'))
            {
                // special MVVM event handling
                var xmlLayoutListItem = source.GetComponentInParent<XmlLayoutListItem>();
                var itemGuid = xmlLayoutListItem.guid;

                if (xmlLayoutListItem != null)
                {
                    var xmlLayoutListComponent = xmlLayoutListItem.GetComponentInParent<XmlLayoutList>();
                    var list = xmlLayoutListComponent.list;

                    var type = this.GetType();
                    var method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (method != null)
                    {
                        var methodParameters = method.GetParameters();

                        if (methodParameters.Length > 0)
                        {
                            var parameterInfo = methodParameters.FirstOrDefault();
                            var parameterType = parameterInfo.ParameterType;

                            if (parameterType == typeof(int))
                            {
                                if (value.EndsWith(".index}"))
                                {
                                    value = list.GetIndexByGUID(itemGuid).ToString();
                                }
                            }
                            else if (parameterType == typeof(string))
                            {
                                if (value.EndsWith(".guid}"))
                                {
                                    value = itemGuid;
                                }
                            }
                            else if (parameterType.IsSubclassOf(typeof(ObservableListItem)))
                            {
                                if (value.EndsWith(".item}"))
                                {
                                    method.Invoke(this, new object[] { list.GetItemByGUID(itemGuid) });

                                    // unlike with the other statements, we've handled this so we don't need to continue to base.ReceiveMessage
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            base.ReceiveMessage(methodName, value, source);
        }
    }
}
#endif
