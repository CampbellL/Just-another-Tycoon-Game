using UnityEngine;
using System.Collections;
using System;
using UI.Xml;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Xml
{
    public static class XmlLayoutFactory
    {
        /// <summary>
        /// Create an XmlLayout instance using the specified Xml file (view), along with an optional controller type
        /// </summary>
        /// <param name="parent">The RectTransform this XmlLayout will be a child of</param>
        /// <param name="xmlFilePath">The path to the Xml file (in a resources folder)</param>
        /// <param name="controllerType">The type of controller to use (optional)</param>
        /// <param name="hidden">If this is set to true, the XmlLayout will not be visible until you call Show()</param>
        /// <returns></returns>
        public static XmlLayout Instantiate(RectTransform parent, string xmlFilePath, Type controllerType = null, bool hidden = false)
        {
            var xmlLayout = InstantiatePrefab("XmlLayout Prefabs/XmlLayout").GetComponent<XmlLayout>();

            // attach the new XmlLayout to the specified parent
            xmlLayout.transform.SetParent(parent);

            // Unity has a habit of setting seemingly random RectTransform values;
            // this fixes that
            FixInstanceTransform(xmlLayout.transform as RectTransform);

            // assign the xml file                        
            if(!string.IsNullOrEmpty(xmlFilePath)) xmlLayout.XmlFile = XmlLayoutUtilities.LoadResource<TextAsset>(xmlFilePath);            

            // instantiate the controller if necessary
            if (controllerType != null)
            {
                xmlLayout.gameObject.AddComponent(controllerType);
            }

            xmlLayout.name = "XmlLayout";

            // Load the new Xml file and build the layout
            xmlLayout.ReloadXmlFile();

            if (hidden)
            {
                xmlLayout.XmlElement.Visible = true;

                // If the XmlLayout has a hide animation set, calling Hide() will trigger it
                // But what we really want here is for the XmlLayout to be invisible from the start
                // So what we do here is get/add a CanvasGroup, and set it to be fully transparent
                // Once the hide animation is complete, we then restore it to regular opacity (which the user will not see, as the game object will no longer be active)
                var canvasGroup = xmlLayout.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = xmlLayout.gameObject.AddComponent<CanvasGroup>();
                
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;

                xmlLayout.Hide(() => 
                {                    
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = true;
                });
            }

            return xmlLayout;
        }

        private static GameObject InstantiatePrefab(string name)
        {
            var prefab = XmlLayoutUtilities.LoadResource<GameObject>(name);
            var gameObject = GameObject.Instantiate(prefab) as GameObject;            

            return gameObject;
        }

        private static void FixInstanceTransform(RectTransform instanceTransform)
        {
            instanceTransform.localPosition = Vector3.zero;            
            instanceTransform.rotation = new Quaternion();
            instanceTransform.localScale = Vector3.one;
            instanceTransform.anchoredPosition = Vector2.zero;
            instanceTransform.anchoredPosition3D = Vector3.zero;            
            instanceTransform.sizeDelta = Vector3.one;
        }
    }
}
