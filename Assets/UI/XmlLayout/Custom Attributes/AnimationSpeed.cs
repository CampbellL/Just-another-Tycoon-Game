using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml.CustomAttributes
{    
    public class AnimationSpeedAttribute: CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {            
            var speed = float.Parse(value);

            var animator = xmlElement.GetComponent<Animator>();
            if (animator == null) animator = xmlElement.gameObject.AddComponent<Animator>();

            animator.speed = speed;

            // xmlElement.CanvasGroup will create the CanvasGroup object if it doesn't already exist, and we may need one
            if (xmlElement.CanvasGroup) { }

            // turns out calling this actually stops the default animation from working? ...okay
            //animator.StartPlayback();                  
        }

        public override eAttributeGroup AttributeGroup
        {
            get
            {
                return eAttributeGroup.Animation;
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "1";
            }
        }
    }            
}
