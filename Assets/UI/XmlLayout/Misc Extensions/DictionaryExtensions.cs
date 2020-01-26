using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Add the specified value to the dictionary if the key doesn't already have a value. If there is already a value stored, then do nothing.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddIfKeyNotExists<TKey, TValue>(this IDictionary<TKey,TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
        }

        /// <summary>
        /// Set the specified value in the dictionary. If the key is not already present in the dictionary, it will be added.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
            else dictionary[key] = value;
        }
    }
}
