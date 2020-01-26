using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;

class AnimationExampleController : XmlLayoutController
{
    private void Start()
    {
        HideThenShow(true);
    }

    private void HideThenShow(bool initial = false)
    {
        xmlLayout.GetElementById("grid").Hide();

        XmlLayoutTimer.DelayedCall(initial ? 0.5f : 3f, () =>
        {
            xmlLayout.GetElementById("grid").Show();

            // repeat
            XmlLayoutTimer.DelayedCall(3f, () => HideThenShow(false), this);
        }, this);
    }
}