#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;

namespace UI.Xml
{
    public class XmlLayoutViewModelChild<T> : RealProxy
        where T : XmlLayoutViewModelChild
    {
        private readonly XmlLayoutViewModelChild _instance;
        private Dictionary<PropertyInfo, string> calculatedListGUIDs = new Dictionary<PropertyInfo, string>();

        private XmlLayoutViewModelChild(T instance)
            :base(typeof(T))
        {
            _instance = instance;
        }

        public static T Create(T instance)
        {
            return (T)new XmlLayoutViewModelChild<T>(instance).GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            var isSetMethod = method.Name.StartsWith("set_");

            object result = null;
            var type = typeof(T);


            if (isSetMethod)
            {
                var propertyName = method.Name.Replace("set_", string.Empty);
                var propertyInfo = type.GetProperties()
                                       .First(p => p.Name == propertyName);

                var valueBefore = propertyInfo.GetValue(_instance, XmlLayoutUtilities.BindingFlags, null, null, null);

                result = method.Invoke(_instance, methodCall.InArgs);

                var valueAfter = propertyInfo.GetValue(_instance, XmlLayoutUtilities.BindingFlags, null, null, null);

                if (valueBefore != valueAfter)
                {
                    _instance.NotifyPropertyChanged(propertyInfo);
                }
            }
            else if (method.Name == "FieldSetter")
            {
                // Note: this isn't being called. It looks like we'll have to use properties instead of fields.

                var fieldName = methodCall.Args[1].ToString();
                var newValue = methodCall.Args[2];
                var fieldInfo = type.GetFields()
                                    .First(f => f.Name == fieldName);

                var valueBefore = fieldInfo.GetValue(_instance);

                result = method.Invoke(_instance, methodCall.InArgs);

                // Seems like we need to manually set the value using reflection (method.Invoke isn't doing anything here)
                fieldInfo.SetValue(_instance, newValue);

                if ((valueBefore == null && newValue != null) || !valueBefore.Equals(newValue))
                {
                    _instance.NotifyFieldChanged(fieldInfo);
                }
            }
            else
            {
                if (method.Name.StartsWith("get_"))
                {
                    var propertyName = method.Name.Replace("get_", string.Empty);
                    var propertyInfo = type.GetProperties()
                                           .First(p => p.Name == propertyName);

                    // Special handling for calculated list properties
                    if (propertyInfo.PropertyType.GetInterface("IObservableList") != null)
                    {
                        if (!propertyInfo.IsAutoProperty())
                        {
                            // Calculated ObservableLists will not automatically preserve their guids, as they are technically a 'new'
                            // list every time they are accessed. This code overrides their 'new' guids with their original values.
                            var list = (IObservableList)propertyInfo.GetValue(_instance, XmlLayoutUtilities.BindingFlags, null, null, null);
                            var listGUIDEntry = calculatedListGUIDs.ContainsKey(propertyInfo) ? calculatedListGUIDs[propertyInfo] : null;
                            if (listGUIDEntry == null)
                            {
                                var guid = list.guid;
                                calculatedListGUIDs.Add(propertyInfo, guid);
                            }
                            else
                            {
                                list.guid = listGUIDEntry;
                            }

                            return new ReturnMessage(list, null, 0, methodCall.LogicalCallContext, methodCall);
                        }
                    }
                }

                result = method.Invoke(_instance, methodCall.InArgs);
            }

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }
    }
}
#endif
