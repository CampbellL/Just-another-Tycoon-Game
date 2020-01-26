using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace UI.Xml.Patching
{
    [InitializeOnLoad]
    class PatchHandler : ScriptableObject
    {
        static PatchHandler m_Instance = null;

        static PatchHandler()
        {
            EditorApplication.update += OnInit;
        }

        static void OnInit()
        {
            EditorApplication.update -= OnInit;

            m_Instance = FindObjectOfType<PatchHandler>();

            if (m_Instance == null)
            {
                m_Instance = CreateInstance<PatchHandler>();
            }

            m_Instance.Run();
        }

        void Run()
        {
            string patchFilePath = Directory.GetParent(Application.dataPath) + "/ProjectSettings/XmlLayoutSettings.asset";
            if (!File.Exists(patchFilePath))
            {
                File.Create(patchFilePath).Close();
            }

            var patchInfoCollection = new List<XmlLayoutPatch>();
            foreach(var line in File.ReadAllLines(patchFilePath))
            {
                // if there is an entry, then it was applied
                patchInfoCollection.Add(new XmlLayoutPatch() { identifier = line, applied = true });
            }

            var basePatchType = typeof(XmlLayoutPatch);
            var patchTypes = basePatchType.Assembly.GetTypes().Where(type => type.IsSubclassOf(basePatchType));

            bool updateStoredPatchInfo = false;

            foreach(var patchType in patchTypes)
            {
                var patchInstance = (XmlLayoutPatch)Activator.CreateInstance(patchType);
                var storedPatchInfo = patchInfoCollection.FirstOrDefault(pi => pi.identifier == patchInstance.identifier);

                if (storedPatchInfo == null || storedPatchInfo.applied == false)
                {
                    Debug.Log("[XmlLayout] Applying patch 'v" + patchInstance.identifier + "'");
                    patchInstance.Apply();

                    if (storedPatchInfo == null)
                    {
                        storedPatchInfo = patchInstance;
                        patchInfoCollection.Add(storedPatchInfo);
                    }

                    storedPatchInfo.applied = true;
                    updateStoredPatchInfo = true;
                }
            }

            if (updateStoredPatchInfo)
            {
                var dataString = String.Empty;
                foreach(var patchInfo in patchInfoCollection)
                {
                    if (patchInfo.applied) dataString += patchInfo.identifier + "\r\n";
                }

                File.WriteAllText(patchFilePath, dataString);
            }
        }
    }
}
