using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UI.Xml
{
    public class XmlAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (XmlLayoutResourceDatabase.instance != null) XmlLayoutResourceDatabase.instance.UpdateCustomResourceDatabases();

            var xmlFiles = importedAssets.Where(a => a.EndsWith(".xml")).ToList();

            foreach (var xmlFile in xmlFiles)
            {
                XmlLayoutEditorUtilities.XmlFileUpdated(xmlFile);
            }
        }
    }
}
