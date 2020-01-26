using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    [ElementTagHandler("Animation")]
    public class AnimationTagHandler : ElementTagHandler
    {
        public override bool isCustomElement
        {
            get { return true; }
        }

        public override string elementChildType
        {
            get { return "none"; }
        }

        public override string extension
        {
            get { return "blank"; }
        }

        public override List<string> attributeGroups
        {
            get { return new List<string>(); }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"name", "xs:string"},
                    {"type", "Normal,Chained,Simultaneous"},
                    {"from", "xs:string"}, // optional (but recommended)
                    {"to", "xs:string"},
                    {"attribute", "xs:string"},
                    {"valueType", "float,Vector2,Vector3,Color"},
                    {"duration", "xs:float"},
                    {"animations", "xs:string"},
                    {"curve", "Linear,EaseInOut,ExpoEaseIn,ExpoEaseOut,CubicEaseIn,CubicEaseOut,ElasticEaseIn,ElasticEaseOut,BounceEaseOut,BounceEaseIn,BounceEaseInOut,BounceEaseOutIn,BackEaseOut,BackEaseIn" }
                };
            }
        }

        public override bool renderElement
        {
            get { return false; }
        }

        public override string elementGroup
        {
            get { return "defaultsOnly"; }
        }

        public override string prefabPath
        {
            get { return null; }
        }

        public override bool allowAnyAttribute
        {
            get
            {
                return false;
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
        }
    }
}