#if UIOBJECT3D_PRESENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.ThreeDimensional;

namespace UI.Xml.Tags
{
    public class DragRotateUIObject3DTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform.transform.parent.GetComponent<UIObject3D>() == null)
                {
                    Debug.LogError("[XmlLayout][UIObject3D][DragRotateUIObject3D] Error: The DragRotateUIObject3D element may only be used as a child of a UIObject3D element.");
                    return null;
                }

                var rotateObject = currentInstanceTransform.parent.GetComponent<DragRotateUIObject3D>();
                if (rotateObject == null) rotateObject = currentInstanceTransform.parent.gameObject.AddComponent<DragRotateUIObject3D>();

                return rotateObject;
            }
        }
        
        public override string prefabPath { get { return null; } }        

        /// <summary>
        /// This is a custom element, add it to the XSD file
        /// </summary>
        public override bool isCustomElement { get { return true; } }

        public override bool renderElement { get { return false; } }
        
        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                   {"rotateX", "xs:boolean"},
                   {"invertX", "xs:boolean"},
                   {"rotateY", "xs:boolean"},
                   {"invertY", "xs:boolean"},
                   {"sensitivity", "xs:float"}
                };
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // The UIObject3DImage must block raycasts otherwise the drag functionality will not work
            var uiObject3D = currentInstanceTransform.transform.parent.GetComponent<UIObject3D>();
            var uiObject3DImage = uiObject3D.GetComponent<UIObject3DImage>();
            uiObject3DImage.raycastTarget = true;

            base.ApplyAttributes(attributesToApply);
        }
    }
}
#endif
