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
    /// This is an example of a ViewModel which can be used by XmlLayout's new MVVM functionality
    /// </summary>
    public class MVVMExampleOptionsMenuViewModel : XmlLayoutViewModel
    {
        /// <summary>
        /// This defines a list of options for the 'Resolution' drop down list.
        /// </summary>
        public ObservableList<string> resolutionOptions { get; set; }
        /// <summary>
        /// This defines the selected value for the 'Resolution' drop down list.
        /// </summary>
        public string resolution { get; set; }

        /// <summary>
        /// This defines a list of options for the 'Quality' drop down list.
        /// </summary>
        public ObservableList<string> qualityOptions { get; set; }
        /// <summary>
        /// This defines the selected value for the 'Quality' drop down list.
        /// </summary>
        public string quality { get; set; }

        /// <summary>
        /// This defines the value for the 'Master Volume' slider
        /// </summary>
        public float masterVolume { get; set; }

        /// <summary>
        /// This defines the value for the 'Music Volume' slider
        /// </summary>
        public float musicVolume { get; set; }

        /// <summary>
        /// This defines the value of the 'Sfx Volume' slider
        /// </summary>
        public float sfxVolume { get; set; }

        /// <summary>
        /// This defines whether or not the 'Enable Hints' toggle is selected
        /// </summary>
        public bool enableHints { get; set; }
    }
}
#endif
