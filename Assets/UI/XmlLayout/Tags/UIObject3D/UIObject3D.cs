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
    public class UIObject3DTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<UIObject3D>();                                
            }
        }
        
        public override string prefabPath
        {
            get
            {
                return "XmlLayout Prefabs/UIObject3D/UIObject3D";
            }
        }        

        /// <summary>
        /// This is a custom element, add it to the XSD file
        /// </summary>
        public override bool isCustomElement { get { return true; } }
        
        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    // UIObject3D properties
                    {"objectPrefab", "xs:string"},
                    {"targetRotation", "xmlLayout:vector3"},
                    {"targetOffset", "xmlLayout:vector2"},
                    {"cameraFOV", "xs:float"},
                    {"cameraDistance", "xs:float"},
                    {"backgroundColor", "xmlLayout:color"},
                    {"limitFrameRate", "xs:boolean"},
                    {"frameRateLimit", "xs:int"},
                    {"renderConstantly", "xs:boolean"},
                    {"enableCameraLight", "xs:boolean"},
                    {"lightColor", "xmlLayout:color"},
                    {"lightIntensity", "xs:float"},

                    // image properties
                    {"color", "xmlLayout:color"},
                    {"raycastTarget", "xs:boolean"},
                    {"imageType", "Simple,Filled"},
                    {"fillMethod", "Horizontal,Vertical,Radial90,Radial180,Radial360"},
                    {"fillOrigin", "Top,Left,Bottom,Right"},
                    {"fillAmount","xs:float"},
                    {"clockwise", "xs:boolean"}
                };
            }
        }

        public override void Open(AttributeDictionary elementAttributes)
        {
            if (primaryComponent == null)
            {
                currentInstanceTransform.gameObject.AddComponent<UIObject3D>();
                currentInstanceTransform.gameObject.AddComponent<UIObject3DImage>();
                currentInstanceTransform.gameObject.AddComponent<LayoutElement>();
            }
        }
        
        public override void Close()
        {
            var uiObject3D = primaryComponent as UIObject3D;
            XmlLayoutTimer.DelayedCall(0.01f, () => uiObject3D.HardUpdateDisplay(), uiObject3D);
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            base.ApplyAttributes(attributesToApply);

            var uiObject3D = primaryComponent as UIObject3D;

            if (attributesToApply.ContainsKey("targetOffset"))
            {
                uiObject3D.TargetOffset = attributesToApply["targetOffset"].ToVector2();                
            }
        }

        public override void SetValue(string newValue, bool fireEventHandlers = true)
        {
            ApplyAttributes(new AttributeDictionary()
            {
                { "objectPrefab", newValue }
            });
        }
    }
}
#endif
