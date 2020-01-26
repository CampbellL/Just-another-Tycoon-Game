using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UI.Xml
{
    public class XmlLayoutResourceDatabaseProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {            
            if(XmlLayoutResourceDatabase.instance != null) XmlLayoutResourceDatabase.instance.LoadResourceData();
        }
    }
}
