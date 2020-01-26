using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Xml;
using UnityEditor;

namespace UI.Xml
{
    public static class XmlLayoutEditorUtilities
    {
        [UnityEditor.Callbacks.DidReloadScripts()]
        public static void Reset()
        {
            XmlLayoutUtilities.Reset();
        }

        public static void XmlFileUpdated(string xmlFile)
        {
            var canvasObjects = GameObject.FindObjectsOfType<Canvas>();

            foreach (var canvas in canvasObjects)
            {
                if (canvas == null) continue;

                var xmlLayoutElements = canvas.GetComponentsInChildren<XmlLayout>(true);

                var elementsUsingThisFile = xmlLayoutElements.Where(xl => xl.AutomaticallyReloadXmlFileIfItChanges)
                                                             .Where(xl =>
                                                                {
                                                                    // if the Xml file contains this path
                                                                    if (xl.XmlFile != null && AssetDatabase.GetAssetPath(xl.XmlFile).Contains(xmlFile)) return true;

                                                                    // or, if any child element xml files is contained in this path
                                                                    if (xl.ChildElementXmlFiles.Any(cx => xmlFile.Contains(cx))) return true;

                                                                    return false;
                                                                })
                                                             .ToList();

                foreach (var xmlLayoutElement in elementsUsingThisFile)
                {
                    RebuildXmlLayout(xmlLayoutElement, () => xmlLayoutElement.ReloadXmlFile(), xmlFile);
                    //xmlLayoutElement.ReloadXmlFile();
                }

                var elementsUsingThisDefaultsFile = xmlLayoutElements.Where(xl => xl.DefaultsFiles != null && xl.DefaultsFiles.Any(df => AssetDatabase.GetAssetPath(df) == xmlFile))
                                                                     .ToList();

                xmlFile = xmlFile.Replace(".xml", string.Empty);

                var elementsIncludingThisFile = xmlLayoutElements.Where(xl => xl.IncludedFiles != null && xl.IncludedFiles.Any(f => xmlFile.EndsWith(f)))
                                                                 .ToList();

                var elementsToRebuild = new List<XmlLayout>();
                elementsToRebuild.AddRange(elementsUsingThisDefaultsFile);
                elementsToRebuild.AddRange(elementsIncludingThisFile);
                elementsToRebuild = elementsToRebuild.Distinct().ToList();

                foreach (var xmlLayoutElement in elementsToRebuild)
                {
                    RebuildXmlLayout(xmlLayoutElement, () => xmlLayoutElement.RebuildLayout(true), xmlFile + ".xml");
                    //xmlLayoutElement.RebuildLayout(true);
                }
            }
        }

        private static void RebuildXmlLayout(XmlLayout xmlLayout, Action rebuildAction, string xmlFile)
        {
            var isActive = xmlLayout.gameObject.activeSelf;

            //var isActiveInHierarchy = xmlLayout.gameObject.activeInHierarchy;
                // I'm not sure it would be wise to activate potentially multiple parent gameobjects
                // If changes are made to an Xml file and they need to be applied to an XmlLayout which is embedded in an inactive parent object,
                // then the best option for now is to either:
                //  a) Set XmlLayout.ForceRebuildOnAwake to true (which will force a rebuild when the XmlLayout is actived)
                // or b) Manually activate the XmlLayout, and click 'Force Rebuild Now' in the editor

            Debug.Log("[XmlLayout] Rebuilding XmlLayout '" + xmlLayout.name + "' as the Xml file '" + xmlFile.Substring(xmlFile.LastIndexOf('/') + 1) + "' has been changed.");
            //EditorGUIUtility.PingObject(xmlLayout.gameObject);

            // The layout code works best if the gameobject is active
            if (!isActive)
            {
                xmlLayout.gameObject.SetActive(true);

                EditorApplication.delayCall += () =>
                {
                    if (xmlLayout != null)
                    {
                        try
                        {
                            rebuildAction();
                        }
                        catch
                        {
                            /*
                             This executes when importing the package for some reason in Unity 2018, which causes exceptions.
                             I haven't been able to find a way to prevent it from executing, so instead we're going to have to just catch those exceptions.
                             Fortunately, the execeptions don't break anything (this code shouldn't be executing at that point anyway), but I don't want them showing up anyway.
                             */
                        }

                        EditorApplication.delayCall += () =>
                        {
                            if (xmlLayout != null) xmlLayout.gameObject.SetActive(false);
                        };
                    }
                };
            } else
            {
                rebuildAction();
            }
        }

        public static void AddSymbol(string symbol)
        {
            foreach (BuildTargetGroup platform in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (IsObsolete(platform) || platform == BuildTargetGroup.Unknown) continue;

#if UNITY_5_6_OR_NEWER
                if (platform == BuildTargetGroup.Switch) continue;
#endif

                var existingSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                var symbols = existingSymbolsString.Replace(" ", string.Empty).Split(';').ToList();

                if (!symbols.Contains(symbol))
                {
                    symbols.Add(symbol);

                    var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newSymbolsString);
                }
            }
        }

        public static void RemoveSymbol(string symbol)
        {
            foreach (BuildTargetGroup platform in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (IsObsolete(platform) || platform == BuildTargetGroup.Unknown) continue;

#if UNITY_5_6_OR_NEWER
                if (platform == BuildTargetGroup.Switch) continue;
#endif

                var existingSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                var symbols = existingSymbolsString.Replace(" ", string.Empty).Split(';').ToList();

                if (symbols.Contains(symbol))
                {
                    symbols.Remove(symbol);

                    var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newSymbolsString);
                }
            }
        }

        // http://stackoverflow.com/questions/29832536/check-if-enum-is-obsolete
        static bool IsObsolete(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (ObsoleteAttribute[])
                fi.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return (attributes != null && attributes.Length > 0);
        }
    }
}
