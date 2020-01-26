using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    [Serializable]
    public class XmlLayoutResourceEntry
    {
        /// <summary>
        /// The string path used by XmlLayout to reference this resource
        /// </summary>
        [SerializeField]
        public string path;

        /// <summary>
        /// The resource referenced by this entry
        /// </summary>
        [SerializeField]
        public UnityEngine.Object resource;
    }
}
