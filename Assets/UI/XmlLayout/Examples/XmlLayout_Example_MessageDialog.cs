using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Collections;

namespace UI.Xml.Examples
{    
    public class XmlLayout_Example_MessageDialog : XmlLayoutController
    {
        XmlElementReference<Text> titleText;
        XmlElementReference<Text> messageText;
        
        void Awake()
        {                        
            titleText = XmlElementReference<Text>("titleText");
            messageText = XmlElementReference<Text>("messageText");            
        }

        public void Show(string title, string text)
        {
            xmlLayout.Show();

            // Because this dialog may not have been active yet at this point,
            // we need to wait a frame to make sure that the XmlLayout has finished setting up,
            // and that the titleText and messageText objects have been populated by Start()
            StartCoroutine(DelayedShow(title, text));            
        }

        protected IEnumerator DelayedShow(string title, string text)
        {            
            yield return new WaitForEndOfFrame();
            
            titleText.element.text = title;
            messageText.element.text = text;
        }        

        public void AppendText(string newText)
        {
            Show(this.titleText.element.text, messageText.element.text + "\r\n\r\n" + newText);            
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            // start with the root XmlElement
            Localize(xmlLayout.XmlElement);
        }

        private void Localize(XmlElement element)
        {
            if (element.attributes.ContainsKey("localized"))
            {
                // localize this element using the dictionary
            }

            if (element.childElements.Count > 0)
            {
                foreach (var child in element.childElements) 
                {
                    // skip ChildXmlLayouts (and consequently their children)
                    if (child.tagType == "ChildXmlLayout") continue;

                    Localize(child);
                }                
            }
        }
    }
}
