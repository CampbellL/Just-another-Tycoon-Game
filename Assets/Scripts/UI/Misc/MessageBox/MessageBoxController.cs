using UI.Xml;

namespace UI.Misc.MessageBox
{
    struct MessageBoxInfo
    {
        private string _heading;
        private string _body;
        private string _buttonText;

        public MessageBoxInfo(string heading, string body, string buttonText)
        {
            this._heading = heading;
            this._body = body;
            this._buttonText = buttonText;
        }
    }
    class MessageBoxController : XmlLayoutController
    {
        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            // ParseXmlResult.Changed   => The Xml was parsed and the layout changed as a result
            // ParseXmlResult.Unchanged => The Xml was unchanged, so no the layout remained unchanged
            // ParseXmlResult.Failed    => The Xml failed validation

            // Called whenever the XmlLayout finishes rebuilding the layout
            // Use this function to make any dynamic changes (e.g. create dynamic lists, menus, etc.) or dynamically load values/selections for elements such as DropDown
        }

        public override void PostLayoutRebuilt()
        {
            base.PostLayoutRebuilt();
        }

        public void SetContent(MessageBoxInfo content)
        {
            
        }
    }
}