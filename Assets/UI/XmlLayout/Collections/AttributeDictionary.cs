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
    public class AttributeDictionary : SerializableDictionary<string, string>
    {
        public AttributeDictionary(IDictionary<string, string> attributes = null)
        {
            _Comparer = StringComparer.OrdinalIgnoreCase;

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    this.SetValue(attribute.Key, attribute.Value);
                }
            }
        }

        public AttributeDictionary Clone()
        {
            return new AttributeDictionary(this);
        }

        public virtual AttributeDictionary AsReadOnly()
        {
            return new ReadOnlyAttributeDictionary(this);
        }

        public virtual string GetValue(string key)
        {
            if (this.ContainsKey(key)) return this[key];

            return null;
        }

        public virtual T GetValue<T>(string key)
        {
            return GetValue(key).ChangeToType<T>();
        }

        public override string ToString()
        {
            var s = "AttributeDictionary Values:\n";

            foreach (var kvp in this)
            {
                s += String.Format("[{0}] => '{1}'\n", kvp.Key, kvp.Value);
            }

            return s;
        }

        public void Merge(AttributeDictionary other)
        {
            if (other.Count == 0) return;

            foreach(var otherKV in other)
            {
                this.SetValue(otherKV.Key, otherKV.Value);
            }
        }
    }
}
