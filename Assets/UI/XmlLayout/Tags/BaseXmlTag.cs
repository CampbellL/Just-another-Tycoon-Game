using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI.Xml.Tags
{
    public abstract class BaseXmlTagHandler : ElementTagHandler
    {
        public override bool isCustomElement { get { return true; } }

        public override string prefabPath { get { return null; } }

        /// <summary>
        /// Override this to specify the Xml to use for this tag
        /// (If not using XmlPath)
        /// </summary>
        public virtual string Xml
        {
            get
            {
                if (XmlPath != null)
                {
                    return XmlLayoutUtilities.LoadResource<TextAsset>(XmlPath).text;
                }

                return "<XmlLayout></XmlLayout>";
            }
        }

        /// <summary>
        /// Override this to specify the path to an Xml file to use for this tag
        /// (Must be accessible with a resource database)
        /// </summary>
        public virtual string XmlPath
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Override this to specify the controller type of this tag
        /// (Must be type inherited from XmlLayoutController)
        /// </summary>
        public virtual Type ControllerType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Override this to specify the primart component type of this tag
        /// (Must be type inherited from MonoBehaviour)
        /// </summary>
        public virtual Type PrimaryComponentType
        {
            get
            {
                return null;
            }
        }

        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (PrimaryComponentType == null) return currentXmlElement;

                return (MonoBehaviour)currentXmlElement.GetComponent(PrimaryComponentType);
            }
        }

        private static AttributeDictionary emptyAttributeDictionary = new AttributeDictionary();

        public override XmlElement GetInstance(RectTransform parent, XmlLayout xmlLayout, string overridePrefabPath = null)
        {
            var xmlElement = base.GetInstance(parent, xmlLayout, overridePrefabPath);

            xmlElement.gameObject.AddComponent<LayoutElement>();

            ApplyAttributes(emptyAttributeDictionary);

            return xmlElement;
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            MatchParentDimensions();

            var _currentXmlElement = currentXmlElement;

            _currentXmlElement.name = this.tagType;

            base.ApplyAttributes(attributesToApply);

            var childLayout = _currentXmlElement.GetComponent<XmlLayout>();
            if (childLayout == null)
            {
                childLayout = _currentXmlElement.gameObject.AddComponent<XmlLayout>();
                childLayout.ForceRebuildOnAwake = false;
            }

            var oldXml = childLayout.Xml;
            childLayout.Xml = Xml;

            if (!childLayout.Xml.Contains("<XmlLayout"))
            {
                childLayout.Xml = "<XmlLayout>" + childLayout.Xml + " </XmlLayout>";
            }

            if (PrimaryComponentType != null)
            {
                var _primaryComponent = _currentXmlElement.GetComponent(PrimaryComponentType);
                if (_primaryComponent == null) _currentXmlElement.gameObject.AddComponent(PrimaryComponentType);
            }

            if (ControllerType != null)
            {
                if (ControllerType.IsSubclassOf(typeof(XmlLayoutController)))
                {
                    var controller = _currentXmlElement.GetComponent<XmlLayoutController>();
                    if (controller == null) _currentXmlElement.gameObject.AddComponent(ControllerType);
                }
                else
                {
                    Debug.LogWarning("[XmlLayout][" + this.GetType().Name + "][Warning]:: Type '" + ControllerType.Name + "' is not inherited XmlLayoutController.");
                }
            }

            // only rebuild if necessary
            if (oldXml != childLayout.Xml) childLayout.RebuildLayout();

            if (!String.IsNullOrEmpty(XmlPath))
            {
                currentXmlLayoutInstance.ChildElementXmlFiles.Add(XmlPath);
            }
        }
    }
}
