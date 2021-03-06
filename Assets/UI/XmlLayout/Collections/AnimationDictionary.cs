using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UI.Xml
{
    [Serializable]
    public class AnimationDictionary : SerializableDictionary<string, XmlLayoutAnimation>
    {
        public AnimationDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }
}
