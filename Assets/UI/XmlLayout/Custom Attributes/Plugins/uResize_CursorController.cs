# if URESIZE_PRESENT
using UnityEngine;
using System;
using DigitalLegacy.UI.Sizing;

namespace UI.Xml.CustomAttributes
{
    public class uResize_CursorController_EnabledAttribute : uResize_CursorControllerAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "false"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);

            uResize_CursorController.enabled = value.ToBoolean();
        }
    }

    public class uResize_CursorController_SetCursorOnStartAttribute : uResize_CursorControllerAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "false"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);

            uResize_CursorController.SetCursorOnStart = value.ToBoolean();
        }
    }

    public class uResize_CursorController_VerticalCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.VerticalCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_HorizontalCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.HorizontalCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_TopLeftCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.TopLeftCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_TopRightCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.TopRightCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_BottomLeftCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.BottomLeftCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_BottomRightCursorAttribute : uResize_CursorControllerAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.BottomRightCursor = value.ToTexture2D();
        }
    }

    public class uResize_CursorController_CursorModeAttribute : uResize_CursorControllerAttribute
    {
        public override string ValueDataType { get { return "Auto,ForceSoftware"; } }
        public override string DefaultValue { get { return "Auto"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.CursorMode = (CursorMode)Enum.Parse(typeof(CursorMode), value);
        }
    }

    public class uResize_CursorController_RegularCursorHotspotAttribute : uResize_CursorControllerAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:vector2"; } }
        public override string DefaultValue { get { return "0 0"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.RegularCursorHotspot = value.ToVector2();
        }
    }

    public class uResize_CursorController_ResozeCursorHotspotAttribute : uResize_CursorControllerAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:vector2"; } }
        public override string DefaultValue { get { return "16 16"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize_CursorController.ResizeCursorHotspot = value.ToVector2();
        }
    }

    public abstract class uResize_CursorControllerAttribute: CustomXmlAttribute
    {        
        protected uResize_CursorController uResize_CursorController
        {
            get;
            private set;
        }

        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            uResize_CursorController = xmlElement.GetComponent<uResize_CursorController>();

            if (uResize_CursorController == null)
            {
                uResize_CursorController = xmlElement.gameObject.AddComponent<uResize_CursorController>();
            }
        }        
    }
}
#endif
