using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace UI.Xml
{
    [CustomEditor(typeof(XmlLayoutLocalization))]
    public class LocalizationDictionaryInspector : Editor
    {
        private XmlLayoutLocalization.LocalizationDictionary _strings;
        private XmlLayoutLocalization.LocalizationDictionary strings
        {
            get
            {
                if (_strings == null) _strings = ((XmlLayoutLocalization)target).strings;
                return _strings;
            }
        }

        public override void OnInspectorGUI()
        {
            var style = new GUIStyle(EditorStyles.toolbar);
            style.alignment = TextAnchor.MiddleCenter;

            XmlLayoutLocalization.LocalizationDictionary tempDictionary = new XmlLayoutLocalization.LocalizationDictionary();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(24));
            EditorGUILayout.LabelField("Key", style, GUILayout.Width(200));
            EditorGUILayout.LabelField("Value", style);
            EditorGUILayout.EndHorizontal();

            bool dictionaryFixRequired = false;

            for (var x = 0; x < strings.Count; x++)
            {
                try
                {
                    var kvp = strings.ElementAt(x);


                    EditorGUILayout.BeginHorizontal();

                    bool remove = (GUILayout.Button("X", GUILayout.Width(24)));

                    var newKey = EditorGUILayout.TextField(kvp.Key, GUILayout.Width(200));
                    var newValue = EditorGUILayout.TextField(kvp.Value);

                    if (!remove)
                    {
                        tempDictionary.Add(newKey, newValue);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                catch (IndexOutOfRangeException)
                {
                    // if this happens, there's something wrong with the source dictionary, perhaps it was badly serialized?
                    dictionaryFixRequired = true;
                }
            }

            if (dictionaryFixRequired)
            {
                FixSavedLocalizationDictionary(tempDictionary);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(24));
            EditorGUI.BeginDisabledGroup(strings.ContainsKey(""));
            if (GUILayout.Button("Add New String", GUILayout.Width(200)))
            {
                if (!strings.ContainsKey(""))
                {
                    try
                    {
                        strings.Add("", "");
                        tempDictionary.Add("", "");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        dictionaryFixRequired = true;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (dictionaryFixRequired)
            {
                FixSavedLocalizationDictionary(tempDictionary);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(24));
            if (GUILayout.Button("Copy Keys to Clipboard", GUILayout.Width(200)))
            {
                EditorGUIUtility.systemCopyBuffer = "xmlLayoutLocalizationKeys=" + String.Join(",", strings.Select(k => k.Key).ToArray());
            }

            EditorGUI.BeginDisabledGroup(!EditorGUIUtility.systemCopyBuffer.StartsWith("xmlLayoutLocalizationKeys="));
            if (GUILayout.Button("Paste Keys from Clipboard"))
            {
                var keysString = EditorGUIUtility.systemCopyBuffer.Replace("xmlLayoutLocalizationKeys=", "");
                var keys = keysString.Split(',').ToList();

                bool confirmed = true;
                if (strings.Any())
                {
                    confirmed = EditorUtility.DisplayDialog("Override existing keys?", "This will replace any existing keys. Are you sure you wish to continue?", "Continue", "Cancel");
                }

                if (confirmed)
                {
                    tempDictionary.Clear();
                    foreach (var key in keys)
                    {
                        tempDictionary.Add(key, strings.ContainsKey(key) ? strings[key] : "");
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                strings.Clear();
                foreach (var kvp in tempDictionary)
                {
                    strings.Add(kvp.Key, kvp.Value);
                }

                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(24));
            if (GUILayout.Button("Save Changes", GUILayout.Width(200)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();

                // update any XmlLayout instances using this localization file
                XmlLayout[] layouts = GameObject.FindObjectsOfType<XmlLayout>();
                foreach (var layout in layouts)
                {
                    if (layout.LocalizationFile == this.target)
                    {
                        layout.RebuildLayout(true, false);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void FixSavedLocalizationDictionary(XmlLayoutLocalization.LocalizationDictionary tempDictionary)
        {
            Debug.LogWarning("[XmlLayout][LocalizationDictionary] Localization dictionary appears corrupt. Attempting to repair...");
            // clear the reference
            _strings = null;
            // replace the original dictionary
            ((XmlLayoutLocalization)target).strings = new XmlLayoutLocalization.LocalizationDictionary();
            // copy the temporary dictionary into the new 'source' dictionary
            foreach (var kvp in tempDictionary)
            {
                strings.Add(kvp.Key, kvp.Value);
            }
            // let Unity know the target has changed and needs to be saved
            EditorUtility.SetDirty(target);

            Debug.LogWarning("[XmlLayout][LocalizationDictionary] Repair complete.");

            return;
        }

    }
}
