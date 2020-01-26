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
    public class NavigationAttribute : CustomXmlAttribute
    {        
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var selectable = xmlElement.GetComponent<Selectable>();

            if (selectable != null)
            {
                var navigation = selectable.navigation;
                navigation.mode = (Navigation.Mode)Enum.Parse(typeof(Navigation.Mode), value);
                selectable.navigation = navigation;
            }
        }

        public override string ValueDataType
        {
            get
            {
                return "None,Horizontal,Vertical,Automatic,Explicit";
            }
        }

        public override string DefaultValue
        {
            get
            {
                return "Automatic";
            }
        }
    }

    public abstract class SelectableAttribute : CustomXmlAttribute
    {
        protected Selectable FindElement(XmlElement fromElement, string desiredElementId)
        {
            return fromElement.xmlLayoutInstance.GetElementById<Selectable>(desiredElementId);
        }        
    }

    public class SelectOnUpAttribute : SelectableAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var selectable = xmlElement.GetComponent<Selectable>();

            if (selectable != null)
            {                                
                // We're delaying here because the element we're referencing may not yet have been parsed
                // at this point in time
                XmlLayoutTimer.AtEndOfFrame(() =>
                    {
                        var navigation = selectable.navigation;
                        navigation.selectOnUp = FindElement(xmlElement, value);
                        selectable.navigation = navigation; 
                    }, xmlElement, true);
            }
        }
    }

    public class SelectOnDownAttribute : SelectableAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var selectable = xmlElement.GetComponent<Selectable>();

            if (selectable != null)
            {                
                // We're delaying here because the element we're referencing may not yet have been parsed
                // at this point in time
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    var navigation = selectable.navigation;
                    navigation.selectOnDown = FindElement(xmlElement, value);
                    selectable.navigation = navigation;
                }, xmlElement, true);
            }
        }
    }

    public class SelectOnLeftAttribute : SelectableAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var selectable = xmlElement.GetComponent<Selectable>();

            if (selectable != null)
            {                
                // We're delaying here because the element we're referencing may not yet have been parsed
                // at this point in time
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    var navigation = selectable.navigation;
                    navigation.selectOnLeft = FindElement(xmlElement, value);
                    selectable.navigation = navigation;
                }, xmlElement, true);
            }
        }
    }

    public class SelectOnRightAttribute : SelectableAttribute
    {
        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            var selectable = xmlElement.GetComponent<Selectable>();

            if (selectable != null)
            {                
                // We're delaying here because the element we're referencing may not yet have been parsed
                // at this point in time
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    var navigation = selectable.navigation;
                    navigation.selectOnRight = FindElement(xmlElement, value);
                    selectable.navigation = navigation;
                }, xmlElement, true);
            }
        }
    }
}
