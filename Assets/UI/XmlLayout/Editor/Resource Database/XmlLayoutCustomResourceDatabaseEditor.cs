using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UI.Xml
{
    [CustomEditor(typeof(XmlLayoutCustomResourceDatabase))]
    public class XmlLayoutCustomResourceDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as XmlLayoutCustomResourceDatabase;
            var originalPrefix = instance.PathPrefix;

            base.OnInspectorGUI();

            // If the prefix changed, update all entries
            if(instance.MonitorContainingFolder && instance.AutomaticallyRemoveEntries && originalPrefix != instance.PathPrefix)
            {
                instance.entries.Clear();
                instance.LoadFolders();
            }

            EditorGUILayout.Space();            

            if (GUILayout.Button("Clear Entries"))
            {
                instance.entries.Clear();
                instance.LoadFolders();
            }

            if (GUILayout.Button("Clear Folders"))
            {
                instance.folders.Clear();
            }

            if (GUILayout.Button("Refresh"))
            {
                instance.LoadFolders();
            }
        }
    }
}
