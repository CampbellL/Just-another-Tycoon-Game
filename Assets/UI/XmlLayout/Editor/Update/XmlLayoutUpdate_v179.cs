// This class is now obsolete and has been replaced by the new XmlLayout patching system

/*
 * As of XmlLayout v1.79, Xml files now use the 'XmlLayout' namespace rather than 'http://www.w3schools.com'
 * This class will locate all Xml files in the project and replace references to the original namespace with the new one.
 * Note: this will execute once each time the project is loaded.
 */

 /*
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UI.Xml.Updates
{
    [InitializeOnLoad]
    public class XmlLayoutUpdate_V179 : ScriptableObject
    {
        static XmlLayoutUpdate_V179 m_Instance = null;

        static XmlLayoutUpdate_V179()
        {
            EditorApplication.update += OnInit;
        }

        static void OnInit()
        {
            EditorApplication.update -= OnInit;

            m_Instance = FindObjectOfType<XmlLayoutUpdate_V179>();

            if (m_Instance == null)
            {
                m_Instance = CreateInstance<XmlLayoutUpdate_V179>();

                UpdateXmlFiles();
            }
        }

        static void UpdateXmlFiles()
        {
            var xmlFiles = Directory.GetFiles(Application.dataPath, "*.xml", SearchOption.AllDirectories);
            var saveChanges = false;

            foreach (var xmlFile in xmlFiles)
            {
                var fileContents = File.ReadAllText(xmlFile);
                var updatedFileContents = fileContents.Replace("http://www.w3schools.com", "XmlLayout")
                                                      .Replace("xsi:noNamespaceSchemaLocation=\"", "xsi:schemaLocation=\"XmlLayout ");

                if (fileContents != updatedFileContents)
                {
                    File.WriteAllText(xmlFile, updatedFileContents);
                    saveChanges = true;
                }
            }

            if (saveChanges)
            {
                AssetDatabase.Refresh();
            }
        }
    }
}
*/