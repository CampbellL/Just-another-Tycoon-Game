using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml.Examples
{
    [ExecuteInEditMode]
    class XmlLayout_Example_ExampleMenu : XmlLayoutController
    {
        /// <summary>
        /// Populated in the editor
        /// </summary>
        public List<XmlLayout> Examples = new List<XmlLayout>();

        protected XmlLayout CurrentExample = null;

        private XmlElement menuButtonGroup;

        public void SelectExample(string name = null)
        {
            if (name == null)
            {
                CurrentExample = null;
                HideAllExamples();

                return;
            }

            var newExample = Examples.FirstOrDefault(e => e.name == name);

            if (newExample != null)
            {
                if (CurrentExample != null && newExample != CurrentExample)
                {
                    // Hide the current example, then call ShowExample(newExample) when the hide animation is complete
                    CurrentExample.Hide(() => ShowExample(newExample));
                }
                else
                {
                    ShowExample(newExample);
                }
            }
            else
            {
                // Special handling, this is a different scene
                switch (name)
                {
                    case "Drag & Drop":
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Drag & Drop Example");
                        break;
                    case "Localization":
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Localization Example");
                        break;
                    case "World Space":
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("World Space Example");
                        break;
                }
            }
        }

        void ShowExample(XmlLayout newExample)
        {
            // Hiding all but the current example helps prevent issues where the user rapidly selects multiple examples at once
            foreach (var example in Examples)
            {
                if (example != newExample) example.Hide();
            }

            newExample.Show();

            CurrentExample = newExample;
        }

        public void HideAllExamples()
        {
            foreach (var example in Examples)
            {
                if (example.gameObject.activeInHierarchy) example.Hide();
            }
        }

        /// <summary>
        /// LayoutRebuilt is called by XmlLayout whenever the layout is finished being built
        /// (regardless of whether the rebuild was triggered automatically or manually)
        /// </summary>
        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            if (parseResult != ParseXmlResult.Changed) return;

            // get the menu button container
            menuButtonGroup = xmlLayout.GetElementById("menuButtons");

            // get the menu button template so that we can clone it
            var menuButtonTemplate = xmlLayout.GetElementById("menuButtonTemplate");

            foreach (var example in Examples)
            {
                var name = example.name;

                AddMenuButton(name, menuButtonGroup, menuButtonTemplate);
            }

            AddMenuButton("Drag & Drop", menuButtonGroup, menuButtonTemplate);
            AddMenuButton("Localization", menuButtonGroup, menuButtonTemplate);
            AddMenuButton("World Space", menuButtonGroup, menuButtonTemplate);
        }

        void AddMenuButton(string name, XmlElement menuButtonGroup, XmlElement menuButtonTemplate)
        {
            // Create a copy of the template
            var menuButton = GameObject.Instantiate(menuButtonTemplate);
            menuButton.name = name;

            // Access the XmlElement component and initialise it for this new button
            var xmlElement = menuButton.GetComponent<XmlElement>();
            xmlElement.Initialise(xmlLayout, (RectTransform)menuButton.transform, menuButtonTemplate.tagHandler);

            // Add the xmlElement to the menuButtonGroup
            menuButtonGroup.AddChildElement(menuButton);

            // Set the necessary attributes, and Apply them
            xmlElement.SetAttribute("text", name);
            xmlElement.SetAttribute("active", "true");  // the template is inactive (so as not to be visible), so we need to activate this button
            xmlElement.SetAttribute("onClick", "SelectExample(" + name + ");"); // Call the SelectExample function (in this XmlEventReceiver)

            xmlElement.SetAttribute("tooltip", "Show the <color=\"#00FF00\">" + name + "</color> example.");
            xmlElement.SetAttribute("tooltipPosition", "Right");
            xmlElement.SetAttribute("tooltipOffset", "15");
            xmlElement.ApplyAttributes();
        }
    }
}
