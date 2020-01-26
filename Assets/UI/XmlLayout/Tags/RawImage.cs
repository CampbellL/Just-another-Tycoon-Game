using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public class RawImageTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<RawImage>();
            }
        }

        public override bool isCustomElement 
        { 
            get { return true; } 
        }
        
        public override string prefabPath
        {
            get { return null; }
        }

        public override string extension
        {
            get { return "blank"; }
        }

        public override List<string> attributeGroups
        {
            get
            {
                return new List<string>()
                {
                    "rectTransform",
                    "rectPosition",
                    "layoutElement",
                    "tooltip",
                    "animation"
                };
            }
        }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"texture", "xs:string"},
                    {"color", "xmlLayout:color"},
                    {"material", "xs:string"},
                    {"raycastTarget", "xs:boolean"},
                    {"uvRect", "xmlLayout:rect"}
                };
            }
        }

        public override void Open(AttributeDictionary elementAttributes)
        {
            currentInstanceTransform.gameObject.AddComponent<RawImage>();
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            MatchParentDimensions();

            base.ApplyAttributes(attributesToApply);
        }
    }
}
