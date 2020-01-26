using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace UI.Xml.Examples
{    
    [ExecuteInEditMode]
    class XmlLayout_Example_Options : XmlLayoutController
    {
        /// <summary>
        /// This is a reference to another XmlEventReceiver object, which is set up to act as a simple modal message dialog
        /// (The reference is added in the editor)
        /// </summary>
        public XmlLayout_Example_MessageDialog MessageDialog = null;

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {            
            SetFormDefaults();
        }

        void SetFormDefaults()
        {            
            // You can easily set form defaults using code by getting hold of the elements by their ids, and then setting their value as needed

            // Note: this is completely optional - Form defaults can also be set directly in the Xml itself (but this allows you to set values programmatically)
            
            // var masterVolumeSlider = xmlLayout.GetElementById<Slider>("masterVolume");
            // masterVolumeSlider.value = 0;        

            var resolutionDropdown = xmlLayout.GetElementById<Dropdown>("resolution");
            resolutionDropdown.SetOptions("1920x1080", "960x600", "1024x768", "800x600");       // SetOptions is an extension method located in the UI.Xml namespace
            resolutionDropdown.SetSelectedValue("960x600");                                     // SetSelectedValue is an extension method located in the UI.Xml namespace

            var qualityDropdown = xmlLayout.GetElementById<Dropdown>("quality");
            qualityDropdown.SetOptions(QualitySettings.names);
            qualityDropdown.value = QualitySettings.GetQualityLevel();

            // SetFormDefaults changes the values of some of the elements on the page, which in turn triggers their event handlers,
            // which results in the 'Apply' button being highlighted by FormChanged()
            ClearApplyButtonHighlight();
        }

        void FormChanged()
        {
            // A value in the form has been changed, so we're going to highlight the 'Apply' button so that the user knows that they have to click it to save their changes
            var applyButton = xmlLayout.GetElementById("applyButton");            

            applyButton.RemoveClass("disabled");                        
        }

        void ClearApplyButtonHighlight()
        {
            var applyButton = xmlLayout.GetElementById("applyButton");

            applyButton.AddClass("disabled");            
        }

        void ResetForm()
        {
            // Rebuild the Xml Elements
            xmlLayout.RebuildLayout(true);
            
            // this will also call LayoutRebuilt()                      
        }

        void SubmitForm()
        {            
            // xmlLayout.GetFormData() returns the values of all form objects in the layout with an 'id' set (as a Dictionary<string, string>)
            var formValues = xmlLayout.GetFormData();

            // As this is only an example, we're not going to actually use these values - instead, we'll just format them into a human-readable string and show the user
            // (with the exception of the Quality setting)
            string formattedFormValues = "<b>Form Values</b>:\n----------------------------------------\n";

            foreach (var formValue in formValues)
            {
                formattedFormValues += String.Format("<b>{0}</b>: <i>{1}</i>\n", FormatFieldName(formValue.Key), formValue.Value);
            }

            formattedFormValues += "\n\n";
            formattedFormValues += "For the purposes of this example, only the <i>Quality</i> setting will take effect.";

            // Show the formatted values in a message dialog (which is also an XmlLayout)
            MessageDialog.Show("Form Submitted", formattedFormValues);

            // Retrieve the index of the selected quality level from QualitySettings.names and set the new value
            var qualitySetting = QualitySettings.names.ToList().IndexOf(formValues["quality"]);
            QualitySettings.SetQualityLevel(qualitySetting);

            // The changes have now been 'applied', so we can clear the highlight
            ClearApplyButtonHighlight();
        }

        /// <summary>
        /// Convert a variable name from 'thisPattern' to 'This Pattern'
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        string FormatFieldName(string fieldName)
        {
           var s = new string(fieldName.ToCharArray().SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new char[] { ' ', c } : new char[] { c }).ToArray());

           s = char.ToUpper(s[0]) + s.Substring(1);

           return s;
        }        

    }
}
