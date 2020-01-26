using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Xml
{
    [Serializable]
    public class XmlLayoutAnimation
    {
        /// <summary>
        /// Type of this animation:
        /// Normal: uses config
        /// Chained:
        /// </summary>
        public eAnimationType type = eAnimationType.Normal;

        /// <summary>
        /// The attribute to target
        /// </summary>
        public string attribute = string.Empty;

        /// <summary>
        /// The value to change the attribute to (by the end of the animation)
        /// </summary>
        public object valueTo = null;

        /// <summary>
        /// The value to change the attribute to at the beginning of the animation (if null, will use the current value)
        /// </summary>
        public object valueFrom = null;

        /// <summary>
        /// Has a value been specified for 'from'?
        /// </summary>
        public bool hasValueFrom = false;

        /// <summary>
        /// What type of value is valueTo
        /// </summary>
        public Type valueType = typeof(float);

        /// <summary>
        /// How long this animation should take to play
        /// </summary>
        public float duration = 0.25f;

        /// <summary>
        /// What curve should we use?
        /// Options for now:
        /// Linear / EaseInOut
        /// (Using a string so as to allow custom curves to be defined in the future)
        /// </summary>
        public string curve = "Linear";

        /// <summary>
        /// List of 'other' animations to use
        /// (if type == Chained or Simultaneous)
        /// </summary>
        public List<string> animations = new List<string>();

        public enum eAnimationType
        {
            Normal,
            Chained,
            Simultaneous
        }

        public XmlLayoutAnimation(AttributeDictionary attributes)
        {
            if (attributes.ContainsKey("type"))
            {
                type = (eAnimationType)Enum.Parse(typeof(eAnimationType), attributes["type"]);
            }

            if (attributes.ContainsKey("attribute"))
            {
                attribute = attributes["attribute"];
            }

            if (attributes.ContainsKey("duration"))
            {
                duration = attributes["duration"].ToFloat();
            }

            if (attributes.ContainsKey("animations"))
            {
                animations = attributes["animations"].ToClassList();
            }

            if (!attributes.ContainsKey("valueType"))
            {
                attributes.Add("valueType", "float");
            }

            switch (attributes["valueType"])
            {
                case "float":
                    valueType = typeof(float);
                    break;
                case "Vector2":
                    valueType = typeof(Vector2);
                    break;
                case "Vector3":
                    valueType = typeof(Vector3);
                    break;
                case "Color":
                    valueType = typeof(Color);
                    break;
            }

            if (attributes.ContainsKey("to"))
            {
                valueTo = attributes["to"].ChangeToType(valueType);
            }

            if (attributes.ContainsKey("from"))
            {
                valueFrom = attributes["from"].ChangeToType(valueType);
                hasValueFrom = true;
            }

            if (attributes.ContainsKey("curve"))
            {
                curve = attributes["curve"];
            }
        }
    }
}