using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace UI.Xml.CustomAttributes
{
    public abstract class TransitionBaseAttribute : CustomXmlAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override bool RestrictToPermittedElementsOnly
        {
            get
            {
                return true;
            }
        }

        public override List<string> PermittedElements
        {
            get
            {
                return new List<string>()
                {
                    "Button",
                    "InputField",
                    "Slider",
                    "Toggle",
                    "ToggleButton",
                    "Dropdown"
                };
            }
        }
    }

    public class TransitionAttribute: TransitionBaseAttribute
    {                
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            Selectable s = xmlElement.GetComponent<Selectable>();
            
            if (s == null) return;
            
            s.transition = (Selectable.Transition)Enum.Parse(typeof(Selectable.Transition), value);
        }

        public override string ValueDataType
        {
            get
            {
                return "None,ColorTint,SpriteSwap,Animation";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "ColorTint";
            }
        }
    }

    public abstract class SpriteStateAttribute : TransitionBaseAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            Selectable s = xmlElement.GetComponent<Selectable>();

            if (s == null) return;

            s.spriteState = new SpriteState()
            {
                disabledSprite = elementAttributes.GetValue("disabledSprite").ToSprite(),
                highlightedSprite = elementAttributes.GetValue("highlightedSprite").ToSprite(),
                pressedSprite = elementAttributes.GetValue("pressedSprite").ToSprite()
            };            
        }

        public override bool KeepOriginalTag
        {
            get
            {
                return true;
            }
        }        
    }

    public class DisabledSpriteAttribute : SpriteStateAttribute { }
    public class HighlightedSpriteAttribute : SpriteStateAttribute { }
    public class PressedSpriteAttribute : SpriteStateAttribute { }

    public abstract class TransitionAnimationAttribute : TransitionBaseAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            Selectable s = xmlElement.GetComponent<Selectable>();

            if (s == null) return;

            s.animationTriggers = new AnimationTriggers()
            {
                normalTrigger = elementAttributes.GetValue("normalTrigger") ?? "Normal",
                highlightedTrigger = elementAttributes.GetValue("highlightedTrigger") ?? "Highlighted",
                pressedTrigger = elementAttributes.GetValue("pressedTrigger") ?? "Pressed",
                disabledTrigger = elementAttributes.GetValue("disabledTrigger") ?? "Disabled"
            };
        }

        public override bool KeepOriginalTag
        {
            get
            {
                return true;
            }
        }
    }

    public class NormalTriggerAttribute : TransitionAnimationAttribute  { }
    public class HighlightedTriggerAttribute : TransitionAnimationAttribute { }
    public class PressedTriggerAttribute : TransitionAnimationAttribute { }
    public class DisabledTriggerAttribute : TransitionAnimationAttribute { }
}
