/*
 *  This is the new method of processing plugins (for Unity 2018.1 and newer)
 */
#if UNITY_2018_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Reflection;

namespace UI.Xml
{
    public class XmlLayoutPluginProcessor
    {
        static XmlLayoutPluginProcessor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            AssemblyReloadEvents.beforeAssemblyReload += ProcessInstalledPlugins;
        }

        [DidReloadScripts(0)]
        public static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            ProcessInstalledPlugins();
        }

        public static void ProcessInstalledPlugins()
        {
            if (XmlLayoutUtilities.XmlLayoutConfiguration == null) return;

            var installedPlugins = GetInstalledPlugins();

            if (XmlLayoutUtilities.XmlLayoutConfiguration.ManageAssemblyDefinitionsAutomatically)
            {
                ManagePluginAssemblyDefinition(installedPlugins);

                ManageXmlLayoutAssemblyDefinition(installedPlugins);
            }

            ManageSymbolDefinitions(installedPlugins);
        }

        public static void GenerateAssemblyDefinitionFiles()
        {
            var installedPlugins = GetInstalledPlugins();

            ManagePluginAssemblyDefinition(installedPlugins);
            ManageXmlLayoutAssemblyDefinition(installedPlugins);
        }

        static List<Plugin> plugins = new List<Plugin>()
        {
            new Plugin("DigitalLegacy.PagedRect", "Pagination", "PAGEDRECT_PRESENT", "UI.Pagination.PagedRect", "PagedRect.cs"),
            new Plugin("DigitalLegacy.UIObject3D", "UIObject3D", "UIOBJECT3D_PRESENT", "UI.ThreeDimensional.UIObject3D", "UIObject3D.cs"),
            new Plugin("DigitalLegacy.TableLayout", "TableLayout", "TABLELAYOUT_PRESENT", "UI.Tables.TableLayout", "TableLayout.cs"),
            new Plugin("DigitalLegacy.uResize", "uResize", "URESIZE_PRESENT", "DigitalLegacy.UI.Sizing.uResize", "uResize.cs"),
            new Plugin("DigitalLegacy.DatePicker", "DatePicker", "DATEPICKER_PRESENT", "UI.Dates.DatePicker", "DatePicker.cs"),
        };

        static List<Assembly> m_assemblies = null;
        static List<Assembly> assemblies
        {
            get
            {
                if (m_assemblies == null)
                {
                    m_assemblies = LoadAssemblies();
                }

                return m_assemblies;
            }
        }

        public static List<string> GetInstalledPlugins()
        {
            List<string> installedPlugins = new List<string>();

            foreach (var plugin in plugins)
            {
                var path = Directory.GetDirectories(Application.dataPath, plugin.path, SearchOption.AllDirectories)
                                    .FirstOrDefault(d => d.EndsWith("UI" + Path.DirectorySeparatorChar + plugin.path));

                if (path != null)
                {
                    installedPlugins.Add(plugin.name);
                }
                else
                {
                    // use the old check - look for specific classes
                    var type = assemblies.Select(a => a.GetLoadableTypes().Where(t => t.FullName.Equals(plugin.className)).FirstOrDefault())
                                         .FirstOrDefault(t => t != null);

                    if (type != null)
                    {
                        installedPlugins.Add(plugin.name);
                    }
                }
            }

            // check for text mesh pro
            var tmpInstalled = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player)
                                                                          .Any(a => a.name == "Unity.TextMeshPro");

            if (tmpInstalled)
            {
                installedPlugins.Add("Unity.TextMeshPro");
            }

            return installedPlugins;
        }

        static List<Assembly> LoadAssemblies()
        {
            var assemblyNames = XmlLayoutAssemblyHelper.GetAssemblyNames();
            var assemblyList = new List<Assembly>();

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    assemblyList.Add(Assembly.Load(assemblyName));
                }
                catch { }
            }

            return assemblyList;
        }

        private static string GetPluginScriptPath(Plugin plugin)
        {
            return Directory.GetFiles(Application.dataPath, plugin.scriptFileName, SearchOption.AllDirectories).FirstOrDefault(p => !p.Contains("Tags"));
        }

        public static void DeleteAssemblyDefinitionFiles()
        {
            var installedPlugins = GetInstalledPlugins();

            foreach (var installedPlugin in installedPlugins)
            {
                var plugin = plugins.FirstOrDefault(p => p.name == installedPlugin);

                if (plugin == null) continue;

                /*var scriptFilePath = Directory.GetFiles(Application.dataPath, plugin.scriptFileName, SearchOption.AllDirectories)
                              .FirstOrDefault(f => f.EndsWith(plugin.path + Path.DirectorySeparatorChar + plugin.scriptFileName));

                var path = scriptFilePath.Substring(0, scriptFilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);*/

                var path = GetPluginScriptPath(plugin);
                path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                var asmdefPath = path + plugin.name + ".asmdef";
                DeleteFile(asmdefPath);


                var editorAsmDefPath = path + "Editor" + Path.DirectorySeparatorChar + plugin.name + ".Editor.asmdef";
                DeleteFile(editorAsmDefPath);
            }

            // now delete the XmlLayout files
            var xmlLayoutPath = Directory.GetDirectories(Application.dataPath, "XmlLayout", SearchOption.AllDirectories)
                                         .SingleOrDefault(d => d.EndsWith("UI" + Path.DirectorySeparatorChar + "XmlLayout"));

            DeleteFile(xmlLayoutPath + Path.DirectorySeparatorChar + "DigitalLegacy.XmlLayout.asmdef");
            DeleteFile(xmlLayoutPath + Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar + "DigitalLegacy.XmlLayout.Editor.asmdef");
            if (Directory.Exists(xmlLayoutPath + Path.DirectorySeparatorChar + "Examples"))
            {
                DeleteFile(xmlLayoutPath + Path.DirectorySeparatorChar + "Examples" + Path.DirectorySeparatorChar + "DigitalLegacy.XmlLayout.Examples.asmdef");
            }

            AssetDatabase.Refresh();
        }

        static void DeleteFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".meta")) File.Delete(path + ".meta");
        }

        public static void ManagePluginAssemblyDefinition(List<string> installedPlugins)
        {
            bool refreshAssetDatabase = false;

            foreach (var installedPlugin in installedPlugins)
            {
                var plugin = plugins.FirstOrDefault(p => p.name == installedPlugin);

                if (plugin == null) continue;

                var scriptFilePath = GetPluginScriptPath(plugin);

                /*var scriptFilePath = Directory.GetFiles(Application.dataPath, plugin.scriptFileName, SearchOption.AllDirectories)
                                              .FirstOrDefault(f => f.EndsWith(plugin.path + Path.DirectorySeparatorChar + plugin.scriptFileName) && !f.Contains("XmlLayout" + Path.DirectorySeparatorChar + "Tags"));*/

                var path = scriptFilePath.Substring(0, scriptFilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                var asmdefPath = path + plugin.name + ".asmdef";

                // check for the existence of an asmdef file
                if (!File.Exists(asmdefPath))
                {
                    List<string> references = new List<string>();

                    if (plugin.name == "DigitalLegacy.DatePicker") references.Add("DigitalLegacy.TableLayout");

                    references.Add("Unity.ugui");

                    // create one
                    WriteAssemblyDefinitionFile(path, plugin.name, references);
                    refreshAssetDatabase = true;
                }

                // If the plugin has an Editor folder, we need to write an editor asmdef file for them too
                var editorFolderPath = path + "Editor";
                if (Directory.Exists(editorFolderPath))
                {
                    var editorAsmdefPath = editorFolderPath + Path.DirectorySeparatorChar + plugin.name + ".Editor.asmdef";

                    if (!File.Exists(editorAsmdefPath))
                    {
                        WriteAssemblyDefinitionFile(editorFolderPath, plugin.name + ".Editor", new List<string>() { plugin.name, "Unity.ugui", "Unity.ugui.Editor" }, new List<string>() { "Editor" });
                        refreshAssetDatabase = true;
                    }
                }
            }

            if (refreshAssetDatabase) AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static void ManageXmlLayoutAssemblyDefinition(List<string> installedPlugins)
        {
            installedPlugins.Add("Unity.ugui");

            // Locate the XmlLayout installation
            var path = Directory.GetDirectories(Application.dataPath, "XmlLayout", SearchOption.AllDirectories)
                                .SingleOrDefault(d => d.EndsWith("UI" + Path.DirectorySeparatorChar + "XmlLayout"));

            // Write the main assembly definition file
            WriteAssemblyDefinitionFile(path, "DigitalLegacy.XmlLayout", installedPlugins);

            // Write the Editor assembly definition file
            WriteAssemblyDefinitionFile(path + Path.DirectorySeparatorChar + "Editor",
                                        "DigitalLegacy.XmlLayout.Editor",
                                        new List<string>() { "DigitalLegacy.XmlLayout", "Unity.ugui", "Unity.ugui.Editor" },
                                        new List<string>() { "Editor" });

            // If the examples directory exists, write out a separate definition for them too
            if (Directory.Exists(path + Path.DirectorySeparatorChar + "Examples"))
            {
                WriteAssemblyDefinitionFile(path + Path.DirectorySeparatorChar + "Examples",
                                            "DigitalLegacy.XmlLayout.Examples",
                                            new List<string>() { "DigitalLegacy.XmlLayout", "DigitalLegacy.TableLayout", "Unity.ugui" });
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void WriteAssemblyDefinitionFile(string path, string name, List<string> references = null, List<string> includePlatforms = null, List<string> excludePlatforms = null)
        {
            if (references == null) references = new List<string>();
            if (includePlatforms == null) includePlatforms = new List<string>();
            if (excludePlatforms == null) excludePlatforms = new List<string>();

#if !UNITY_2019_1_OR_NEWER
            references.Remove("Unity.ugui");
            references.Remove("Unity.ugui.Editor");
#endif

            var asmdef = new AssemblyDefinition()
            {
                name = name,
                references = references,
                includePlatforms = includePlatforms,
                excludePlatforms = excludePlatforms,
                allowUnsafeCode = false
            };

            File.WriteAllText(path + "/" + name + ".asmdef", JsonUtility.ToJson(asmdef));
        }


        static void ManageSymbolDefinitions(List<string> installedPlugins)
        {
            List<string> platformsUpdated = new List<string>();

            foreach (BuildTargetGroup platform in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (IsObsolete(platform) || platform == BuildTargetGroup.Unknown) continue;

                if (platform == BuildTargetGroup.Switch) continue;

                var existingSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
                var symbols = existingSymbolsString.Replace(" ", string.Empty).Split(';').ToList();

                foreach (var plugin in plugins)
                {
                    if (installedPlugins.Contains(plugin.name))
                    {
                        symbols.Add(plugin.symbol);
                    }
                }

                if (installedPlugins.Contains("Unity.TextMeshPro"))
                {
                    symbols.Add("TEXTMESHPRO_PRESENT");
                }

                var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());

                if (existingSymbolsString != newSymbolsString)
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

        // http://stackoverflow.com/questions/29832536/check-if-enum-is-obsolete
        static bool IsObsolete(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (ObsoleteAttribute[])
                fi.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return (attributes != null && attributes.Length > 0);
        }

        public static void RemoveAllFromCurrentBuildTarget()
        {
            var symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols = symbolString.Replace(" ", string.Empty).Split(';').ToList();

            foreach (var plugin in plugins)
            {
                symbols.Remove(plugin.symbol);
            }

            var newSymbolsString = String.Join(";", symbols.Distinct().ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newSymbolsString);
        }

        class Plugin
        {
            public string name;
            public string path;
            public string symbol;
            public string className;
            public string scriptFileName;

            public Plugin(string name, string path, string symbol, string className, string scriptFileName)
            {
                this.name = name;
                this.path = path;
                this.symbol = symbol;
                this.className = className;
                this.scriptFileName = scriptFileName;
            }
        }

        class AssemblyDefinition
        {
            public string name;
            public List<string> references;
            public List<string> includePlatforms;
            public List<string> excludePlatforms;
            public bool allowUnsafeCode;
        }
    }
}
#endif