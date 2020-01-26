using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;

class XmlLayout_Localization_Example : XmlLayoutController
{
    public string selectedLanguage = "English";
    XmlElementReference<XmlLayoutToggleGroup> toggleGroup = null;

    void Awake()
    {
        xmlLayout.Show();

        toggleGroup = XmlElementReference<XmlLayoutToggleGroup>("languageToggleGroup");        
    }
    
    public override void LayoutRebuilt(ParseXmlResult parseResult)
    {
        if (toggleGroup != null)
        {
            // As the layout has just been rebuilt, the toggle group will no longer have anything selected
            // so we're just going to set it again here     
            toggleGroup.element.SetSelectedValue(selectedLanguage, false);            
        }
    }

    void ChangeLanguage(string language)
    {        
        this.selectedLanguage = language;

        if (language == "No Localization")
        {
            xmlLayout.SetLocalizationFile(null);
            return;
        }

        var languageFile = XmlLayoutUtilities.LoadResource<XmlLayoutLocalization>("Localization/" + language);

        if (languageFile == null)
        {
            Debug.LogWarningFormat("Warning: localization file for language '{0}' not found!", language);
            return;
        }

        xmlLayout.SetLocalizationFile(languageFile);
    }

    void ReturnToMainExamples()
    {
        xmlLayout.Hide(() => { UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ExampleScene"); });
    }
}
