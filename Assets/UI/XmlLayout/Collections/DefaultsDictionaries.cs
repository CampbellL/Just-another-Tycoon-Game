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
    public class ClassAttributeCollectionDictionary : SerializableDictionary<string, AttributeDictionary>
    {
        public class OrderedStringDictionary : SerializableDictionary<string, int> { }
        public OrderedStringDictionary order = new OrderedStringDictionary();
        private int count = 0;

        public ClassAttributeCollectionDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
        
        public override void Add(string key, AttributeDictionary value)
        {
            base.Add(key, value);

            order.Add(key, count++);
        }
    }

    [Serializable]
    public class DefaultAttributeValueDictionary : SerializableDictionary<string, ClassAttributeCollectionDictionary>
    {        
        public DefaultAttributeValueDictionary()
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }
    }
}
