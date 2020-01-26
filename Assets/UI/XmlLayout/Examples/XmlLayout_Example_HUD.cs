using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI.Xml;

namespace UI.Xml.Examples
{
    class XmlLayout_Example_HUD : XmlLayoutController
    {
        public XmlLayout_Example_ExampleMenu ExampleMenu = null;        

        public override void Hide(Action onCompleteCallback = null)
        {
            if (ExampleMenu != null)
            {
                ExampleMenu.SelectExample(null);
            }
            else
            {
                // This code is executed in the World Space example (which is in a different scene to the base Example Scene)
                base.Hide(() => { UnityEngine.SceneManagement.SceneManager.LoadScene("ExampleScene"); });
            }
        }
    }
}
