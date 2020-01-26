using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml
{
    [RequireComponent(typeof(XmlLayout))]
    public partial class XmlLayoutController : MonoBehaviour
    {
        private XmlLayout _xmlLayout;
        /// <summary>
        /// The 'XmlLayout' instance for this XmlLayoutController
        /// </summary>
        public XmlLayout xmlLayout
        {
            get
            {
                if (_xmlLayout == null)
                {
                    _xmlLayout = this.GetComponent<XmlLayout>();
                }

                return _xmlLayout;
            }
        }

        /// <summary>
        /// If this is set to true, then this XmlLayoutController
        /// will not process events.
        /// </summary>
        public bool SuppressEventHandling = false;

        /// <summary>
        /// If this is true, then the XmlLayout is presently rebuilding the layout
        /// </summary>
        public bool LayoutRebuildInProgress { get; set; }

        /// <summary>
        /// By default, XmlLayout events will be called on this XmlLayoutController.
        /// This property allows you to specify a different target for these events.
        /// </summary>
        public object EventTarget { get; set; }

        /// <summary>
        /// This event will be called whenever the layout is rebuilt.
        /// </summary>
        public Action<XmlLayoutController> OnLayoutRebuilt { get; set; }

        public virtual void ReceiveMessage(string methodName, string value, RectTransform source = null, List<string> parameters = null, int pointerId = 0)
        {
            if (SuppressEventHandling) return;

            object eventTarget = this.EventTarget != null ? this.EventTarget : this;

            var type = eventTarget.GetType();
            var method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
            {
                Debug.LogError("[XmlLayout][XmlLayoutController] No method named '" + methodName + "' has been defined in Event Target '" + eventTarget.GetType().Name + "'!");
                return;
            }

            if (value == null || method.GetParameters().Count() == 0)
            {
                method.Invoke(eventTarget, null);
            }
            else
            {
                var methodParameters = method.GetParameters();

                if (methodParameters.Length == 0)
                {
                    method.Invoke(eventTarget, null);
                    return;
                }

                List<object> parametersToInvoke = new List<object>();
                for (var i = 0; i < methodParameters.Length; ++i)
                {
                    if (i > 0)
                    {
                        if (parameters == null || parameters.Count == 0) break;
                    }

                    var parameterInfo = methodParameters[i];
                    var parameterType = parameterInfo.ParameterType;

                    // Some handlers modify the value of 'value' so it may not match the first parameter exactly, so for the first parameter we will use the old 'value' rather than pulling it from the new collection
                    var val = i == 0 ? value : parameters[i];
                    if (!string.IsNullOrEmpty(val) && val.Trim().Equals("pointerId", StringComparison.OrdinalIgnoreCase))
                    {
                        val = pointerId.ToString();
                    }

                    if (source != null && val.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        object parameter = source;

                        // if the parameter is a MonoBehaviour type, then attempt to find an object of that type from the source object and use it as the parameter
                        if (parameterType.IsSubclassOf(typeof(MonoBehaviour)))
                        {
                            parameter = source.GetComponent(parameterType);
                            if (parameter == null)
                            {
                                parameter = source.GetComponentInChildren(parameterType);
                            }
                        }

                        parametersToInvoke.Add(parameter);
                    }
                    else
                    {
                        parametersToInvoke.Add(val.ChangeToType(parameterType, xmlLayout));
                    }
                }

                try
                {
                    method.Invoke(eventTarget, parametersToInvoke.ToArray());
                }
                catch (TargetParameterCountException)
                {
                    Debug.LogError("[XmlLayout][Event Handling] The arguments provided for the '" + methodName + "' method do not match its signature.");
                }

                /*
                var parameterInfo = parameters.FirstOrDefault();
                var parameterType = parameterInfo.ParameterType;

                if (value == "this" && source != null)
                {
                    object parameter = source;

                    // if the parameter is a MonoBehaviour type, then attempt to find an object of that type from the source object and use it as the parameter
                    if (parameterType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        parameter = source.GetComponent(parameterType);
                        if (parameter == null)
                        {
                            parameter = source.GetComponentInChildren(parameterType);
                        }
                    }

                    method.Invoke(eventTarget, new object[] { parameter });
                }
                else
                {
                    method.Invoke(eventTarget, new object[] { value.ChangeToType(parameterType, xmlLayout) });
                }*/
            }
        }

        public virtual void ReceiveElementDroppedMessage(string methodName, XmlElement item, XmlElement droppedOn)
        {
            if (SuppressEventHandling) return;

            var type = this.GetType();
            var method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
            {
                Debug.LogError("[XmlLayout][XmlLayoutController] No method named '" + methodName + "' has been defined in this XmlLayoutController!");
                return;
            }

            object eventTarget = this.EventTarget != null ? this.EventTarget : this;
            method.Invoke(eventTarget, new object[] { item, droppedOn });
        }

        /// <summary>
        /// This function will be called whenever the layout is rebuilt - if you have any setup code which needs to be executed after the layout is rebuilt, override this function and implement it here.
        /// ParseXmlResult.Changed   => The Xml was parsed and the layout changed as a result
        /// ParseXmlResult.Unchanged => The Xml was unchanged, so no the layout remained unchanged
        /// ParseXmlResult.Failed    => The Xml failed validation
        /// </summary>
        public virtual void LayoutRebuilt(ParseXmlResult parseResult)
        {
            if (this.OnLayoutRebuilt != null)
            {
                this.OnLayoutRebuilt(this);
            }
        }

        internal virtual void ViewModelUpdated(bool triggerLayoutRebuild = true)
        {
        }

        public virtual void PreLayoutRebuilt()
        {
            LayoutRebuildInProgress = true;
        }

        public virtual void PostLayoutRebuilt()
        {
            LayoutRebuildInProgress = false;
        }

        public virtual void ViewModelMemberChanged(string propertyName)
        {
        }

        internal virtual string ProcessViewModel(string xml)
        {
            return xml;
        }

        public virtual void Show()
        {
            xmlLayout.Show();
        }

        public virtual void Hide(Action onCompleteCallback = null)
        {
            xmlLayout.Hide(onCompleteCallback);
        }
    }
}
