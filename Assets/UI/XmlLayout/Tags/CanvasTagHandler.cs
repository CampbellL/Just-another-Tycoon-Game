using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public class CanvasTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                return null;
            }
        }

        public override bool isCustomElement { get { return true; } }        

        public override List<string> attributeGroups
        {
            get
            {                
                return new List<string>()
                {
                    "image"
                };
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            if (!attributesToApply.ContainsKey("width") && !attributes.ContainsKey("width")) attributesToApply.Add("width", "100%");
            if (!attributesToApply.ContainsKey("height") && !attributes.ContainsKey("height")) attributesToApply.Add("height", "100%");

            base.ApplyAttributes(attributesToApply);
        }
    }
}
