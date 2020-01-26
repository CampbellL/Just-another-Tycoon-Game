/*
 * As of XmlLayout v1.79, Xml files now use the 'XmlLayout' namespace rather than 'http://www.w3schools.com'
 * This class will locate all Xml files in the project and replace references to the original namespace with the new one.
 * Note: this will execute once each time the project is loaded.
 */

using UnityEngine;
using UnityEditor;
using System.IO;

namespace UI.Xml.Patching
{
    internal class Patch_V179 : XmlLayoutPatch
    {
        [SerializeField]
        public override string identifier { get { return "1.79"; } }

        public override void Apply()
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
