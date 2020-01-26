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
    public class RotateUIObject3DTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform.transform.parent.GetComponent<UIObject3D>() == null)
                {
                    Debug.LogError("[XmlLayout][UIObject3D][RotateUIObject3D] Error: The RotateUIObject3D element may only be used as a child of a UIObject3D element.");
                    return null;
                }

                var rotateObject = currentInstanceTransform.parent.GetComponent<RotateUIObject3D>();
                if (rotateObject == null) rotateObject = currentInstanceTransform.parent.gameObject.AddComponent<RotateUIObject3D>();

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
                    {"rotationMode", "Constant,WhenMouseIsOver,WhenMouseIsOverThenSnapBack"},
                    {"rotateX", "xs:boolean"},
                    {"rotateXSpeed", "xs:float"},
                    {"rotateY", "xs:boolean"},
                    {"rotateYSpeed", "xs:float"},
                    {"rotateZ", "xs:boolean"},
                    {"rotateZSpeed", "xs:float"},
                    {"snapbackTime", "xs:float"}
                };
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // By default, the rotator has rotateY set to true - for XmlLayout, we don't want that
            if (!attributesToApply.ContainsKey("rotateY") && !currentXmlElement.attributes.ContainsKey("rotateY"))
            {
                var rotateObject = primaryComponent as RotateUIObject3D;
                rotateObject.RotateY = false;
            }

            base.ApplyAttributes(attributesToApply);
        }
    }
}
#endif
