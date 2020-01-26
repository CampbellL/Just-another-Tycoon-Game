using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UI.Xml
{
    [ExecuteInEditMode]
    class XmlLayout_Example_ColorSchemeManager : XmlLayoutController
    {
        public GameObject Root = null;

        private string colorScheme
        {
            get { return PlayerPrefs.GetString("colorScheme"); }
            set { PlayerPrefs.SetString("colorScheme", value); }
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(colorScheme)) colorScheme = "GreenYellow";

            ChangeColorSchemeWithoutRebuild(colorScheme);
        }

        private void Start()
        {
            xmlLayout.GetElementById<Dropdown>("colorSchemeDropdown").SetSelectedValue(colorScheme);

            ChangeColorScheme(colorScheme);
        }

        public void ChangeColorSchemeWithoutRebuild(string newScheme)
        {
            colorScheme = newScheme;

            var colorSchemeFile = XmlLayoutUtilities.LoadResource<TextAsset>(string.Format("Xml/ColorSchemes/{0}", newScheme));

            if (colorSchemeFile == null)
            {
                Debug.LogErrorFormat("[XmlLayout][Example][Color Scheme Manager] Warning: unable to locate color scheme definition '{0}'", newScheme);
                return;
            }

            List<XmlLayout> xmlLayouts = Root.gameObject
                                             .GetComponentsInChildren<XmlLayout>(true)
                                             .ToList();

            foreach (var layout in xmlLayouts)
            {
                // skip this layout
                if (layout == this.xmlLayout) continue;

                if (layout.DefaultsFiles == null) layout.DefaultsFiles = new List<TextAsset>();

                layout.DefaultsFiles.Clear();
                layout.DefaultsFiles.Add(colorSchemeFile);
            }
        }

        public void ChangeColorScheme(string newScheme)
        {
            colorScheme = newScheme;

            var colorSchemeFile = XmlLayoutUtilities.LoadResource<TextAsset>(string.Format("Xml/ColorSchemes/{0}", newScheme));

            if (colorSchemeFile == null)
            {
                Debug.LogErrorFormat("[XmlLayout][Example][Color Scheme Manager] Warning: unable to locate color scheme definition '{0}'", newScheme);
                return;
            }

            List<XmlLayout> xmlLayouts = Root.gameObject
                                             .GetComponentsInChildren<XmlLayout>(true)
                                             .ToList();

            foreach(var layout in xmlLayouts)
            {
                // skip this layout
                if (layout == this.xmlLayout) continue;

                if (layout.DefaultsFiles == null) layout.DefaultsFiles = new List<TextAsset>();

                var inactive = !layout.gameObject.activeSelf;

                if (inactive)
                {
                    layout.gameObject.SetActive(true);
                }

                layout.DefaultsFiles.Clear();
                layout.DefaultsFiles.Add(colorSchemeFile);

                layout.RebuildLayout(true);

                if (inactive)
                {
                    // copy the local variable (if we use 'layout' it will reference the foreach variable which changes through each iteration)
                    var layoutTemp = layout;

                    // hide the layout again at the end of the frame
                    XmlLayoutTimer.AtEndOfFrame(() =>
                    {
                        if (layoutTemp == null) return;

                        //canvasGroup.alpha = alphaBefore;
                        layoutTemp.gameObject.SetActive(false);
                    }, layoutTemp, true);
                }
            }
        }
    }
}