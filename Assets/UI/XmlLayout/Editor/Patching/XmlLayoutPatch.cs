using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml.Patching
{
    internal class XmlLayoutPatch
    {
        public virtual string identifier { get; set; }
        public bool applied = false;
        public virtual void Apply() { }
    }
}
