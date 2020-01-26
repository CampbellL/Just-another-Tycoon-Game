using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Reflection;

namespace UI.Xml.Tags
{
    public class ChildXmlLayoutTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get { return currentInstanceTransform.GetComponentInChildren<XmlLayout>(); }
        }

        // don't use a prefab
        public override string prefabPath
        {
            get { return null; }
        }

        // Generate xsd documentation for this
        public override bool isCustomElement
        {
            get { return true; }
        }

        // No children permitted
        public override string elementChildType
        {
            get { return null; }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"viewPath", "xs:string"},
                    {"controller", "xs:string"}
                };
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // necessary for elements which don't use a prefab
            MatchParentDimensions();

            // If multiple child layouts are nested, then ApplyAttributes() will be called for each before this method has finished executing
            // This becomes an issue, because tag handlers are singletons intended to deal with a single element in one go,
            // which means that the 'currentXmlElement' reference will be replaced with the child, which causes several issues
            // this is also true of all other references, although in this case only 'currentXmlElement' causes any trouble
            // It may be necessary in future to modify the way tag handlers work such that each XmlLayout reference has its own collection of tag handlers,
            // although that will require a small amount of additional memory and processing
            var _currentXmlElement = currentXmlElement;

            _currentXmlElement.name = "ChildXmlLayout";
            if (_currentXmlElement.gameObject.GetComponent<LayoutElement>() == null) _currentXmlElement.gameObject.AddComponent<LayoutElement>();

            base.ApplyAttributes(attributesToApply);

            // Don't pass 'id' on
            attributesToApply.Remove("id");
            attributesToApply.Remove("internalId");

            // if we've already been initialized, don't repeat the process
            if (_currentXmlElement.GetAttribute("initialized") != null)
            {
                // attempt to apply the attributes to the child
                // I've removed this for the time being; as there are potential issues with properties e.g. width="50%" would make
                // the container use width="50%" and then the child would be 50% width of that
                //
                // var _childXmlLayout = _currentXmlElement.childElements.FirstOrDefault(t => t.tagType == "XmlLayout");
                // if(_childXmlLayout != null) _childXmlLayout.ApplyAttributes(attributesToApply);

                return;
            }

            var viewPath = attributesToApply.GetValue<string>("viewPath");

            if (String.IsNullOrEmpty(viewPath))
            {
                Debug.LogWarning("[XmlLayout][Warning][ChildXmlLayout]:: The 'viewPath' attribute is required.");
                return;
            }

            // validate viewPath
            var xmlFile = XmlLayoutResourceDatabase.instance.GetResource<TextAsset>(viewPath);

            if (xmlFile == null)
            {
                Debug.LogWarning("[XmlLayout][Warning][ChildXmlLayout]:: View '" + viewPath + "' not found. Please ensure that the view is accessible via an XmlLayout Resource Database (or is in a Resources folder).");
                return;
            }

            Type controllerType = null;
            var controllerTypeName = attributesToApply.GetValue<string>("controller");

            if (!String.IsNullOrEmpty(controllerTypeName))
            {
                // controllerType = Type.GetType(controllerTypeName, false, true);
                controllerType = GetTypeFromStringName(controllerTypeName);

                if (controllerType == null)
                {
                    Debug.LogWarning("[XmlLayout][Warning][ChildXmlLayout]:: Controller Type '" + controllerTypeName + "' not found. Please ensure that the full class name (including the namespace, if the class is located within one). For example: MyNamespace.MyLayoutControllerType");
                }
            }

            bool passEventsToParentController = false;
            if (controllerType == null)
            {
                controllerType = typeof(XmlLayoutController);
                passEventsToParentController = true;
            }

            var childXmlLayout = XmlLayoutFactory.Instantiate(currentInstanceTransform, viewPath, controllerType);

            childXmlLayout.ParentLayout = _currentXmlElement.xmlLayoutInstance;
            childXmlLayout.ForceRebuildOnAwake = false;

            if (passEventsToParentController)
            {
                childXmlLayout.XmlLayoutController.EventTarget = currentXmlLayoutInstance.XmlLayoutController;
            }

            // Adding a sub-canvas may (slightly) improve performance
            if (_currentXmlElement.gameObject.GetComponent<Canvas>() == null) _currentXmlElement.gameObject.AddComponent<Canvas>();
            if (_currentXmlElement.gameObject.GetComponent<GraphicRaycaster>() == null) _currentXmlElement.gameObject.AddComponent<GraphicRaycaster>();

            childXmlLayout.XmlElement.tagType = "XmlLayout";

            _currentXmlElement.AddChildElement(childXmlLayout.XmlElement, false);

            _currentXmlElement.SetAttribute("initialized", "true");

            childXmlLayout.XmlElement.ApplyAttributes(attributesToApply);

            // For some reason, the child XmlLayout offset Min/Max values are incorrect, so we need to force them to be zero
            if (!attributesToApply.ContainsKey("offsetMax")) childXmlLayout.XmlElement.rectTransform.offsetMax = Vector2.zero;
            if (!attributesToApply.ContainsKey("offsetMin")) childXmlLayout.XmlElement.rectTransform.offsetMin = Vector2.zero;
        }

        private Type GetTypeFromStringName(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
#if NET_4_6
				//looking for types on dynamic assemblies causes problems
                if (assembly.IsDynamic) {
					continue;
                }
#endif
                var type = assembly.GetType(typeName, false, true);
                if (type != null)
                {
                    return type;
                }
            }

            Debug.LogError("Could not find type from string for type:" + typeName);
            return null;
        }
    }
}
