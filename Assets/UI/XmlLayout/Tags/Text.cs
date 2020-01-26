using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace UI.Xml.Tags
{
    public class TextTagHandler : ElementTagHandler
    {
        public static List<string> TextAttributes = new List<string>()
        {
            "text", "fontstyle", "font", "fontsize",
            "horizontalOverflow", "verticalOverflow",
            "resizeTextForBestFit", "resizeTextMinSize", "resizeTextMaxSize",
            "alignByGeometry"
        };

        public override MonoBehaviour primaryComponent
        {
            get
            {
                if (currentInstanceTransform == null) return null;

                return currentInstanceTransform.GetComponent<Text>();
            }
        }

        public override bool UseParseChildElements { get { return true; } }

        public override void ParseChildElements(XmlNode xmlNode)
        {
            var innerXml = xmlNode.InnerXml
                                  .Replace(" xmlns=\"XmlLayout\"", string.Empty)
                                  .Replace(" xmlns=\"http://www.w3schools.com\"", string.Empty)
                                  .Replace("<![CDATA[", string.Empty)
                                  .Replace("]]>", string.Empty);

            innerXml = ReplaceIgnoreCase(innerXml, "<textcolor color=", "<color=");
            innerXml = ReplaceIgnoreCase(innerXml, "</textcolor", "</color");

            innerXml = ReplaceIgnoreCase(innerXml, "<textsize size=", "<size=");
            innerXml = ReplaceIgnoreCase(innerXml, "</textsize", "</size");

            innerXml = innerXml.Trim();

            innerXml = ReplaceIgnoreCase(innerXml, @" {2,}", " ");

            innerXml = innerXml.Replace("<br/>", "\n")
                               .Replace("<br />", "\n");

            if (innerXml.Contains("\n"))
            {
                innerXml = innerXml.Replace("\\n", "\n");

                // iterate through each line of text and trim it
                innerXml = string.Join("\n", innerXml.Split('\n').Select(s => s.Trim()).ToArray());
            }

            innerXml = StringExtensions.DecodeEncodedNonAsciiCharacters(innerXml);

            var textComponent = primaryComponent as Text;
            textComponent.text = innerXml;
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            base.ApplyAttributes(attributesToApply);

            if (attributesToApply.ContainsKey("text"))
            {
                (primaryComponent as Text).text = StringExtensions.DecodeEncodedNonAsciiCharacters(attributesToApply["text"]);
            }
        }

        private static string ReplaceIgnoreCase(string source, string match, string replace)
        {
            return Regex.Replace(source, match, replace, RegexOptions.IgnoreCase);
        }
    }
}
