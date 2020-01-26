#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;
using System.Linq;
using UnityEngine.UI;

namespace UI.Xml.Examples
{
    /// <summary>
    /// This is an example which utilises the new MVVM functionality.
    /// The type of ViewModel it uses is specified as the generic parameter, in this case, it is 'MVVMExampleOptionsMenuViewModel' which is defined in MVVMExampleOptionsMenuViewModel.cs
    /// </summary>
    public class MVVMExampleOptionsMenuController : XmlLayoutController<MVVMExampleOptionsMenuViewModel>
    {
        /// <summary>
        /// Populated in the inspector.
        /// </summary>
        public XmlLayout_Example_MessageDialog MessageDialog = null;

        /// <summary>
        /// This is an XmlElementReference to the 'Apply' button.
        /// This reference will be maintained automatically by XmlLayout between layout rebuilds.
        /// </summary>
        private XmlElementReference<Button> applyButton;

        void Awake()
        {
            // Populate the 'Apply' button reference.
            // (this requires a reference to the XmlLayout controller, so it can only be populated after the instance is created)
            applyButton = XmlElementReference<Button>("applyButton");
        }

        /// <summary>
        /// This method will be called by XmlLayout to load the 'initial' view data.
        /// It will only be called once (although you can call it again in your own code if you wish)
        /// </summary>
        protected override void PrepopulateViewModelData()
        {
            viewModel = new MVVMExampleOptionsMenuViewModel()
            {
                resolutionOptions = new ObservableList<string>() { "960x600", "1024x768", "1920x1080" },
                resolution = "1920x1080",

                qualityOptions = QualitySettings.names.ToObservableList(),
                quality = QualitySettings.names[QualitySettings.GetQualityLevel()],

                masterVolume = 0.80f,
                musicVolume = 0.45f,
                sfxVolume = 0.55f,

                enableHints = true
            };

            // If you wish, you could also manipulate ViewModel properties here as follows:

            /*viewModel.resolutionOptions = new ObservableList<string>()
            {
                "960x600",
                "1024x768",
                "1920x1080"
            };

            viewModel.resolution = "1920x1080";

            viewModel.qualityOptions = QualitySettings.names.ToObservableList();
            viewModel.quality = QualitySettings.names[QualitySettings.GetQualityLevel()];

            viewModel.masterVolume = 0.80f;
            viewModel.musicVolume = 0.45f;
            viewModel.sfxVolume = 0.55f;

            viewModel.enableHints = true;*/
        }

        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever any of the values of any of the elements in the form are changed
        /// </summary>
        void FormChanged()
        {
            // our form data has changed; enable the apply button
            applyButton.element.interactable = true;
        }

        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever the 'Reset' button is clicked
        /// </summary>
        void ResetForm()
        {
            // Our form is tied to our ViewModel; if we restore the ViewModel's data to its initial values,
            // then our form will also be reset to match
            PrepopulateViewModelData();

            // disable the apply button again
            applyButton.element.interactable = false;
        }

        /// <summary>
        /// This is a regular XmlLayout event which has been set up to be called
        /// whenever the 'Apply' button is clicked
        /// </summary>
        void Apply()
        {
            // All of the view model properties in this example use two-way binding, which means that the ViewModel itself is updated
            // whenever any of their values change; as such, we can utilize the 'viewModel' object to see the new values
            MessageDialog.Show("Updated ViewModel Values",
                               String.Format(@"
Resolution    : {0}
Quality       : {1}
Master Volume : {2}
Music Volume  : {3}
Sfx Volume    : {4}
Enable Hints  : {5}
", viewModel.resolution, viewModel.quality, viewModel.masterVolume, viewModel.musicVolume, viewModel.sfxVolume, viewModel.enableHints ));

            // disable the apply button again
            applyButton.element.interactable = false;
        }
    }
}
#endif
