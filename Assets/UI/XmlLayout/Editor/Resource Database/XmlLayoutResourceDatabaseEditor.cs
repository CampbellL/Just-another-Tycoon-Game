using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace UI.Xml
{
    [CustomEditor(typeof(XmlLayoutResourceDatabase))]
    public class XmlLayoutResourceDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var instance = (XmlLayoutResourceDatabase)target;

            foreach (var entry in instance.entries)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(entry.path);
                if(entry.resource != null) EditorGUILayout.LabelField(entry.resource.GetType().Name);

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
