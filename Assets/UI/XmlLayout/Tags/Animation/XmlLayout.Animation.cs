using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml
{
    public partial class XmlLayout
    {
        public AnimationDictionary animations = new AnimationDictionary();

        public void HandleAnimationNode(AttributeDictionary attributes)
        {
            if (!attributes.ContainsKey("name"))
            {
                return;
            }

            animations.SetValue(attributes["name"], new XmlLayoutAnimation(attributes));
        }
    }
}