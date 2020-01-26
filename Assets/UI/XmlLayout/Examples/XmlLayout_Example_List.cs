using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Collections;

namespace UI.Xml.Examples
{    
    class XmlLayout_Example_List : XmlLayoutController
    {
        XmlElementReference<XmlLayoutProgressBar> progressBar;

        void Start()
        {
            progressBar = XmlElementReference<XmlLayoutProgressBar>("progressBar");            
        }

        void Update()
        {
            progressBar.element.percentage += Time.deltaTime * 2.5f;

            if (progressBar.element.percentage >= 100f) progressBar.element.percentage = 0.0f;
        }        
    }
}
