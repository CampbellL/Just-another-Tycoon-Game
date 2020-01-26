#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UI.Xml.Tags;

namespace UI.Xml
{
    public class XmlLayoutViewModel : MarshalByRefObject
    {
        internal XmlLayoutController controller { get; private set; }

        private List<PropertyInfo> calculatedProperties { get; set; }

        protected XmlLayoutViewModel() { }

        internal void Initialise(XmlLayoutController controller)
        {
            this.controller = controller;

            DetectCalculatedProperties();

            ObserveExistingLists();

            InitializeViewModelChildren();
        }

        /// <summary>
        /// Observe any lists which already have a value when the ViewModel is created
        /// </summary>
        private void ObserveExistingLists()
        {
            var type = GetType();

            type.GetMembers()
                .Where(m =>
                {
                    // skip any properties without set methods
                    var property = type.GetProperty(m.Name);
                    if (property != null && property.GetSetMethod() == null) return false;

                    return true;
                })
                .ToList()
                .ForEach(m =>
                {
                    if (m.GetMemberDataType() != null && m.GetMemberDataType().GetInterface("IObservableList") != null)
                    {
                        var value = (IObservableList)m.GetMemberValue(this);
                        if (value != null)
                        {
                            ObserveList(value, m.Name);
                        }
                    }
                });
        }

        /// <summary>
        /// Calculated properties can change if other fields/properties change,
        /// so we need some special handling for them
        /// </summary>
        private void DetectCalculatedProperties()
        {
            calculatedProperties = this.GetType()
                                       .GetProperties()
                                       .Where(p => !p.IsAutoProperty())
                                       .ToList();
        }

        internal static Type ViewModelChildType = null;

        private void InitializeViewModelChildren()
        {
            if (ViewModelChildType == null) ViewModelChildType = typeof(XmlLayoutViewModelChild);

            var viewModelChildrenMembers = GetType().GetMembers()
                                                    .Where(m =>
                                                            {
                                                                return m.GetMemberDataType() != null && m.GetMemberDataType().IsSubclassOf(ViewModelChildType);
                                                            });

            foreach(var member in viewModelChildrenMembers)
            {
                var instance = member.GetMemberValue(this);
                if (instance != null)
                {
                    var child = ((XmlLayoutViewModelChild)instance);
                    child.Initialise(controller, this, member.Name);

                    var proxyType = typeof(XmlLayoutViewModelChild<>);
                    var specialisedProxyType = proxyType.MakeGenericType(new Type[] { member.GetMemberDataType() });
                    var createMethod = specialisedProxyType.GetMethod("Create");

                    var proxy = createMethod.Invoke(null, new object[] { child });

                    member.SetMemberValue(this, proxy);
                }
            }
        }

        public override string ToString()
        {
            var type = this.GetType();
            var fields = type.GetMembers()
                             .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                             .ToDictionary(k => k.Name, v => v.GetMemberValue(this));

            StringBuilder s = new StringBuilder();

            s.Append(type.Name + " => { ");
            s.Append(String.Join(", ", fields.Select(f => f.Key + ": " + (f.Value != null ? ("\"" + f.Value.ToString() + "\"") : "null")).ToArray()));
            s.Append(" }");

            return s.ToString();
        }

        internal void NotifyPropertyChanged(PropertyInfo property)
        {
            if (TypeIsObservableList(property.PropertyType))
            {
                ObserveList((IObservableList)property.GetValue(this, null), property.Name);
            }

            MemberChanged(property.Name);
        }

        internal void NotifyFieldChanged(FieldInfo field)
        {
            if (TypeIsObservableList(field.FieldType))
            {
                ObserveList((IObservableList)field.GetValue(this), field.Name);
            }

            MemberChanged(field.Name);
        }

        public virtual void MemberChanged(string propertyName, bool propogateToCalculatedProperties = true, bool ignoreMainProperty = false)
        {
            //Debug.Log("MemberChanged(" + propertyName + ")");

            if (!ignoreMainProperty) controller.ViewModelMemberChanged(propertyName);

            if (propogateToCalculatedProperties)
            {
                foreach (var property in calculatedProperties)
                {
                    var _property = property;
                    if (property.PropertyType.GetInterface("IObservableList") != null)
                    {
                        // this needs to be delayed to the end of the frame, as MemberChanged()
                        // may be called before the update has been fully processed
                        XmlLayoutTimer.AtEndOfFrame(() =>
                        {
                            UpdateCalculatedListView(_property);
                        }, controller);
                    }
                    else
                    {
                        MemberChanged(property.Name, false);
                    }
                }
            }
        }

        private bool TypeIsObservableList(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ObservableList<>);
        }

        private void ObserveList(IObservableList list, string listName)
        {
            list.itemChanged += (index, item, changedField) => ListItemUpdated(list, index, item, listName, changedField);
            list.itemAdded += (item) => ListItemAdded(list, item, listName);
            list.itemRemoved += (item) => ListItemRemoved(list, item, listName);

            XmlLayoutTimer.AtEndOfFrame(() => MemberChanged(listName), controller);
        }

        private static List<IObservableListTagHandler> _listTagHandlers = null;
        private static List<IObservableListTagHandler> listTagHandlers
        {
            get
            {
                if (_listTagHandlers == null)
                {
                    _listTagHandlers = XmlLayoutUtilities.GetXmlTagHandlers()
                                                         .Where(t => t is IObservableListTagHandler)
                                                         .Cast<IObservableListTagHandler>()
                                                         .ToList();
                }
                return _listTagHandlers;
            }
        }


        private ListTagHandler _listTagHandler;
        private ListTagHandler listTagHandler
        {
            get
            {
                if (_listTagHandler == null) _listTagHandler = (ListTagHandler)XmlLayoutUtilities.GetXmlTagHandler("List"); ;
                return _listTagHandler;
            }
        }

        private DataTableTagHandler _dataTableTagHandler;
        private DataTableTagHandler dataTableTagHandler
        {
            get
            {
                if (_dataTableTagHandler == null) _dataTableTagHandler = (DataTableTagHandler)XmlLayoutUtilities.GetXmlTagHandler("DataTable");
                return _dataTableTagHandler;
            }
        }


        private void ListItemAdded(IObservableList list, object item, string listName)
        {
            var tagHandler = listTagHandlers.FirstOrDefault(t => t.IsHandlingList(list));

            if (tagHandler != null)
            {
                tagHandler.AddListItem(list, item, listName);
                MemberChanged(listName, true, true);
            }
            else
            {
                MemberChanged(listName);
            }
        }

        private void ListItemRemoved(IObservableList list, object item, string listName)
        {
            var tagHandler = listTagHandlers.FirstOrDefault(t => t.IsHandlingList(list));

            if (tagHandler != null)
            {
                tagHandler.RemoveListItem(list, item, listName);
                MemberChanged(listName, true, true);
            }
            else
            {
                MemberChanged(listName);
            }
        }

        private void ListItemUpdated(IObservableList list, int index, object item, string listName, string changedField = null)
        {
            var tagHandler = listTagHandlers.FirstOrDefault(t => t.IsHandlingList(list));

            if (tagHandler != null)
            {
                tagHandler.UpdateListItem(list, index, item, listName, changedField);
                MemberChanged(listName, true, true);
            }
            else
            {
                MemberChanged(listName);
            }
        }

        public void NotifyListChanged(string listName)
        {
            var propertyInfo = this.GetType().GetProperty(listName);

            if (propertyInfo != null)
            {
                UpdateCalculatedListView(propertyInfo);
            }
        }

        private void UpdateCalculatedListView(PropertyInfo property)
        {
            var listElement = listTagHandler.ListElements.FirstOrDefault(l => l.Value.DataSource == property.Name).Value;
            if (listElement != null)
            {
                var list = (IObservableList)property.GetValue(this, null);
                listTagHandler.SetInstance(listElement.rectTransform, listElement.listElement.xmlLayoutInstance);
                listTagHandler.ProcessCalculatedListUpdate(list);
            }
        }

        /// <summary>
        /// Set the value of a ViewModel field or property
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="newValue"></param>
        public void SetValue(string memberName, object newValue)
        {
            MVVMUtilities.SetNestedMemberValue(this.GetType(), memberName, this, newValue, controller.xmlLayout);
            /*
            var memberInfo = GetType().GetMember(memberName).FirstOrDefault();

            if (memberInfo != null)
            {
                var valueBefore = GetValue(memberName);

                if (valueBefore != newValue)
                {
                    var newValueType = newValue.GetType();
                    var memberDataType = memberInfo.GetMemberDataType();

                    if (newValueType == typeof(System.String) && newValueType != memberDataType)
                    {
                        memberInfo.SetMemberValue(this, ((string)newValue).ChangeToType(memberDataType, this.controller.xmlLayout));
                    }
                    else
                    {
                        memberInfo.SetMemberValue(this, newValue);
                    }
                }
            }*/
        }

        /// <summary>
        /// Set a value of an ObservableListItem object in an ObservableList
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="index"></param>
        /// <param name="memberName"></param>
        /// <param name="newValue"></param>
        public void SetListItemValue(string listName, int index, string memberName, object newValue)
        {
            var listMemberInfo = GetType().GetMember(listName).FirstOrDefault();

            if (listMemberInfo != null)
            {
                var list = (IObservableList)listMemberInfo.GetMemberValue(this);
                var item = list[index];

                var itemType = item.GetType();
                var itemMemberInfo = itemType.GetMember(memberName).FirstOrDefault();

                if (itemMemberInfo != null)
                {
                    // TODO:
                    //var valueBefore = GetListItemValue(memberName, index);

                    //if (!valueBefore.Equals(newValue))
                    {
                        var newValueType = newValue.GetType();
                        var memberDataType = itemMemberInfo.GetMemberDataType();

                        if (memberDataType.IsNumericType())
                        {
                            var stringValue = newValue.ToString();
                            float temp = 0;
                            if (String.IsNullOrEmpty(stringValue)
                             || stringValue == "-"
                             || !float.TryParse(stringValue, out temp))
                            {
                                // do not attempt to set the value if it is not a valid number (yet)
                                return;
                            }
                        }

                        if (newValueType == typeof(System.String) && newValueType != memberDataType)
                        {
                            itemMemberInfo.SetMemberValue(item, ((string)newValue).ChangeToType(memberDataType, this.controller.xmlLayout));
                        }
                        else
                        {
                            itemMemberInfo.SetMemberValue(item, newValue);
                        }
                    }
                }

                MemberChanged(listName, true, true);
            }
        }

        /// <summary>
        /// Get the value of a ViewModel field or property
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public object GetValue(string memberName)
        {
            return MVVMUtilities.GetNestedMemberValue(GetType(), memberName, this);
        }
    }
}
#endif
