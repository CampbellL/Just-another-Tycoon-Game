#if UNITY_2018_1_OR_NEWER
using System.Collections.Generic;

namespace UI.Xml.CustomAttributes
{
    public abstract class LayoutGroupBaseAttribute : CustomXmlAttribute
    {
        public override List<string> PermittedElements
        {
            get
            {
                return new List<string>()
                {
                    "HorizontalLayout",
                    "VerticalLayout"
                };
            }
        }

        public override bool KeepOriginalTag { get { return true; } }
    }

    public class ChildControlWidthAttribute : LayoutGroupBaseAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "true"; } }
    }

    public class ChildControlHeightAttribute : LayoutGroupBaseAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "true"; } }
    }

#if UNITY_2019_1_OR_NEWER
    public class ChildScaleWidthAttribute : LayoutGroupBaseAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "false"; } }
    }

    public class ChildScaleHeightAttribute : LayoutGroupBaseAttribute
    {
        public override string ValueDataType { get { return "xs:boolean"; } }
        public override string DefaultValue { get { return "false"; } }
    }
#endif
}
#endif