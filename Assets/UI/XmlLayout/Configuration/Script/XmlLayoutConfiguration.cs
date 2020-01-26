using UnityEngine;
using System.Collections.Generic;

namespace UI.Xml.Configuration
{    
    public class XmlLayoutConfiguration : ScriptableObject
    {
        public Object XSDFile;
        
        /// <summary>
        /// This is used as a base file which then dynamically has custom-attributes and tags added to it before being compiled into XSDFile.
        /// </summary>
        public Object BaseXSDFile;

        [Tooltip("If this is set to true, then XmlLayout automatically update the XSD file when compiling (default = true). If set to false, then you will need to instruct XmlLayout to update the XSD file by clicking the 'Regenerate XSD File Now' button, or the XmlLayout/Regenerate XSD File menu item.")]
        public bool AutomaticallyUpdateXSDFile = true;

        [Tooltip("If this is set to true, then XmlLayout will no longer output a message to the console whenever the XSD file has been updated.")]
        public bool SuppressXSDUpdateMessage = false;

        [Tooltip("If this is set to true, then you will no longer receive Xml validation errors when using non-standard attributes.")]
        public bool AllowAnyAttribute = false;

        [Tooltip("If this is set to true, then XmlLayout will check all available assemblies for custom elements and attributes. If false, then only the assembly containing XmlLayout (and any specified by the 'Custom Assembly List' property) will be checked. Note: changing this property may trigger a recompilation.")]
        public bool ComprehensiveCustomElementAndAttributeCheck = true;

        [Tooltip("If 'Comprehensive Custom Element and Attribute Check' is false, then you can specify additional assemblies to check for custom elements and attributes here. Please use the full name for any assembly.")]
        public List<string> CustomAssemblyList = new List<string>();

        [Tooltip("If 'Comprehensive Custom Element and Attribute Check' is true, then you can specify assemblies to exclude here. Partial names are acceptable - assemblies starting with the names specified here will be excluded.")]
        public List<string> AssemblyExcludeList = new List<string>();

        [Tooltip("If this is set to true, then XmlLayout will handle selectable navigation events (such as when the Tab key is pressed).")]
        public bool UseXmlLayoutSelectableNavigation = true;

#if UNITY_2018_1_OR_NEWER
        [Tooltip("If this is set to true, then XmlLayout will automatically generate assembly definition files for both itself and any installed Digital Legacy Plugins.")]
        public bool ManageAssemblyDefinitionsAutomatically = false;        
#endif
    }
}
