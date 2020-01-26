using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public class ColorTagHandler : ElementTagHandler
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
                    {"color", "xmlLayout:color"}
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

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {            
        }
    }
}
