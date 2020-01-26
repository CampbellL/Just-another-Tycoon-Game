/*
 * This is the old method for processing plugins - used for versions of Unity prior to 2018.1
 */
#if !UNITY_2018_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.Reflection;

namespace UI.Xml
{
    public class XmlLayoutPluginProcessor
    {
        static Dictionary<string, string> plugins = new Dictionary<string, string>()
        {
            {"UI.Pagination.PagedRect", "PAGEDRECT_PRESENT"},
            {"UI.Dates.DatePicker", "DATEPICKER_PRESENT"},
            {"TMPro.TextMeshProUGUI", "TEXTMESHPRO_PRESENT"},
            {"UI.ThreeDimensional.UIObject3D", "UIOBJECT3D_PRESENT"},
            {"DigitalLegacy.UI.Sizing.uResize", "URESIZE_PRESENT" }
        };

        static List<Assembly> assemblies = new List<Assembly>();


        [DidReloadScripts(1)]
        public static void ProcessInstalledPlugins()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            ManageSymbolDefinitions();
        }

        public static void RemoveAllFromCurrentBuildTarget()
        {
            var symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols = symbolString.Replace(" ", string.Empty).Split(';').ToList();

            foreach (var pluginSymbol in plugins.Values)
            {
                symbols.Remove(pluginSymbol);
            }

            var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newSymbolsString);
        }

        static void LoadAssemblies()
        {
            var assemblyNames = XmlLayoutAssemblyHelper.GetAssemblyNames();
            assemblies = new List<Assembly>();

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch { }
            }
        }

        static void ManageSymbolDefinitions()
        {
            LoadAssemblies();

            List<string> platformsUpdated = new List<string>();

            foreach (BuildTargetGroup platform in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if ((platform != BuildTargetGroup.iOS && platform != BuildTargetGroup.WSA) && IsObsolete(platform) || platform == BuildTargetGroup.Unknown) continue;

#if UNITY_5_6_OR_NEWER
                if (platform == BuildTargetGroup.Switch) continue;
#endif

                var existingSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                var symbols = existingSymbolsString.Replace(" ", string.Empty).Split(';').ToList();

                foreach (var plugin in plugins)
                {
                    symbols = ManageSymbolDefinition(symbols, plugin.Key, plugin.Value);
                }

                var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());

                if( existingSymbolsString != newSymbolsString)
                {
                    platformsUpdated.Add(platform.ToString());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newSymbolsString);
                }
            }

            if (platformsUpdated.Any())
            {
                Debug.LogFormat("[XmlLayout] Updating symbols for platforms {0}...", String.Join(", ", platformsUpdated.ToArray()));
            }
        }

        static List<string> ManageSymbolDefinition(List<string> symbols, string className, string symbol)
        {
            var type = assemblies.Select(a => a.GetLoadableTypes().Where(t => t.FullName.Equals(className)).FirstOrDefault())
                                 .FirstOrDefault(t => t != null);

            if (type != null)
            {
                if (!symbols.Contains(symbol)) symbols.Add(symbol);
            }
            else
            {
                if (symbols.Contains(symbol)) symbols.Remove(symbol);
            }

            return symbols;
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
#endif
