#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;

namespace UI.Xml
{
    public abstract partial class ElementTagHandler
    {
        /// <summary>
        /// Used by MVVM functionality to load data from an ObservableList.
        /// Override this functionality to create custom elements which source data
        /// from a list dynamically.
        /// </summary>
        /// <param name="list"></param>
        public virtual void SetListData(IObservableList list)
        {
        }

        protected virtual void HandleDataSourceAttribute(string dataSource, string additionalDataSource = null)
        {
            var dataSourceObject = new XmlElementDataSource(dataSource, currentXmlElement);

            // remove any pre-existing entries (as the dataSource string may have changed)
            currentXmlLayoutInstance.ElementDataSources.RemoveAll(ed => ed.XmlElement == currentXmlElement);

            if (string.IsNullOrEmpty(dataSource)) return;

            currentXmlLayoutInstance.ElementDataSources.Add(dataSourceObject);

            if (dataSourceObject.BindingType == ViewModelBindingType.TwoWay)
            {
                EnableTwoWayBinding(dataSource);
            }

            if (!currentXmlLayoutInstance.rebuildInProgress)
            {
                if (currentXmlLayoutInstance.XmlLayoutController != null && currentXmlLayoutInstance.XmlLayoutController is XmlLayoutControllerMVVM)
                {
                    ((XmlLayoutControllerMVVM)currentXmlLayoutInstance.XmlLayoutController).ViewModelMemberChanged(dataSource);
                }
            }
        }

        protected virtual void EnableTwoWayBinding(string dataSource)
        {
            if (primaryComponent == null) return;

            // Special handling for onValueChanged parameters to allow for two way binding
            var onValueChangedMemberInfo = (primaryComponent.GetType()).GetMember("onValueChanged").FirstOrDefault();

            if (onValueChangedMemberInfo != null)
            {
                var onValueChangedListener = onValueChangedMemberInfo.GetMemberValue(primaryComponent);
                var addListenerMethod = onValueChangedListener.GetType().GetMethod("AddListener");

                var eventType = addListenerMethod.GetParameters()[0].ParameterType;
                var parameterType = eventType.GetGenericArguments()[0];

                var controller = (XmlLayoutControllerMVVM)currentXmlLayoutInstance.XmlLayoutController;

                // I tried to implement this more generically with reflection but had no luck. I'm sure there is a way to do it...
                // I'll try again at a later date.
                if (parameterType == typeof(float))
                {
                    ((UnityEvent<float>)onValueChangedListener).AddListener((v) => controller.SetViewModelValue(dataSource, v, true));
                }
                else if (parameterType == typeof(int))
                {
                    ((UnityEvent<int>)onValueChangedListener).AddListener((v) => controller.SetViewModelValue(dataSource, v, true));
                }
                else if (parameterType == typeof(string))
                {
                    ((UnityEvent<string>)onValueChangedListener).AddListener((v) => controller.SetViewModelValue(dataSource, v, true));
                }
                else if (parameterType == typeof(bool))
                {
                    ((UnityEvent<bool>)onValueChangedListener).AddListener((v) => controller.SetViewModelValue(dataSource, v, true));
                }
                // Do I need to add any additional types here?
            }
        }
    }
}
#endif
