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
    public class ObservableListItemProxy<T> : RealProxy
    {
        private readonly object _instance;
        private readonly IObservableList _list;

        private ObservableListItemProxy(T instance, IObservableList list)
            : base(typeof(T))
        {
            _instance = instance;
            _list = list;
        }

        public static T Create(T instance, IObservableList list)
        {
            return (T)new ObservableListItemProxy<T>(instance, list).GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            var isSetMethod = method.Name.StartsWith("set_");

            object result = null;

            if (isSetMethod)
            {
                var propertyName = method.Name.Replace("set_", string.Empty);
                var propertyInfo = typeof(T).GetProperties()
                                            .First(p => p.Name == propertyName);

                var valueBefore = propertyInfo.GetValue(_instance, XmlLayoutUtilities.BindingFlags, null, null, null);

                result = method.Invoke(_instance, methodCall.InArgs);

                var valueAfter = propertyInfo.GetValue(_instance, XmlLayoutUtilities.BindingFlags, null, null, null);

                if (valueBefore != valueAfter)
                {
                    _list.NotifyItemChanged(_instance, propertyName);
                }
            }
            else if (method.Name == "FieldSetter")
            {
                var fieldName = methodCall.Args[1].ToString();
                var newValue = methodCall.Args[2];
                var fieldInfo = typeof(T).GetFields()
                                         .First(f => f.Name == fieldName);

                var valueBefore = fieldInfo.GetValue(_instance);

                result = method.Invoke(_instance, methodCall.InArgs);

                // Seems like we need to manually set the value using reflection (method.Invoke isn't doing anything here)
                fieldInfo.SetValue(_instance, newValue);

                if ((valueBefore == null && newValue != null) || !valueBefore.Equals(newValue))
                {
                    _list.NotifyItemChanged(_instance, fieldName);
                }
            }
            else
            {
                result = method.Invoke(_instance, methodCall.InArgs);
            }

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }
    }
}
#endif
