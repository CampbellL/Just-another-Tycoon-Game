# if URESIZE_PRESENT
using System;
using DigitalLegacy.UI.Sizing;

namespace UI.Xml.CustomAttributes
{            
    public class uResize_EnabledAttribute: uResizeBooleanAttribute
    {        
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.enabled = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromLeftAttribute : uResizeBooleanAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromLeft = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromRightAttribute : uResizeBooleanAttribute
    {
        public override string DefaultValue { get { return "true"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromRight = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromTopAttribute : uResizeBooleanAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromTop = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromBottomAttribute : uResizeBooleanAttribute
    {
        public override string DefaultValue { get { return "true"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromBottom = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromBottomRightAttribute : uResizeBooleanAttribute
    {
        public override string DefaultValue { get { return "true"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromBottomRight = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromBottomLeftAttribute : uResizeBooleanAttribute
    {        
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromBottomLeft = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromTopLeftAttribute : uResizeBooleanAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromTopLeft = value.ToBoolean();
        }
    }

    public class uResize_AllowResizeFromTopRightAttribute : uResizeBooleanAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AllowResizeFromTopRight = value.ToBoolean();
        }
    }

    public class uResize_KeepWithinParentAttribute : uResizeBooleanAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.KeepWithinParent = value.ToBoolean();
        }
    }

    public class uResize_MinSizeAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:vector2"; } }
        public override string DefaultValue { get { return "0 0"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.MinSize = value.ToVector2();
        }
    }

    public class uResize_MaxSizeAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:vector2"; } }
        public override string DefaultValue { get { return "0 0"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.MaxSize = value.ToVector2();
        }
    }

    public class uResize_ResizeListenerColorAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:color"; } }
        public override string DefaultValue { get { return "rgba(0,0,0,0)"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.ResizeListenerColor = value.ToColor();
        }
    }

    public class uResize_ResizeListenerThicknessAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xs:float"; } }
        public override string DefaultValue { get { return "16"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.ResizeListenerThickness = value.ToFloat();
        }
    }

    public class uResize_AspectRatioControlAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "None,Auto,WidthControlsHeight,HeightControlsWidth"; } }
        public override string DefaultValue { get { return "None"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AspectRatioControl = (eAspectRatioMode)Enum.Parse(typeof(eAspectRatioMode), value);
        }
    }

    public class uResize_DesiredAspectRatioAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xmlLayout:vector2"; } }
        public override string DefaultValue { get { return "1 1"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.DesiredAspectRatio = value.ToVector2();
        }
    }

    public class uResize_AdjustPivotAttribute : uResizeBooleanAttribute
    {
        public override string DefaultValue { get { return "true"; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            base.Apply(xmlElement, value, elementAttributes);
            uResize.AdjustPivot = value.ToBoolean();
        }
    }

    public abstract class uResizeBooleanAttribute : uResizeAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "false"; } }
    }

    public abstract class uResizeAttribute: CustomXmlAttribute
    {        
        protected uResize uResize
        {
            get;
            private set;
        }

        public override bool UsesApplyMethod { get { return true; } }
 
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {
            uResize = xmlElement.GetComponent<uResize>();

            if (uResize == null)
            {
                uResize = xmlElement.gameObject.AddComponent<uResize>();
            }
        }        
    }
}
#endif
