using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Examples
{
    class XmlLayout_Example_TestItOut : XmlLayoutController
    {
        public XmlLayout_Example_MessageDialog MessageDialog = null;
        public XmlLayout_Example_ExampleMenu ExampleMenu = null;

        string Xml = null;

        bool viewPortExpanded = false;

        void Start()
        {
            // Select the Empty example
            Empty();
        }

        public override void Show()
        {
            base.Show();

            ScrollToTop();
        }

        public override void Hide(Action onCompleteCallback = null)
        {
            ExampleMenu.SelectExample(null);
        }

        void UpdateCodeInputField()
        {
            // populate the code input field with the Example Xml
            var codeInputField = xmlLayout.GetElementById<InputField>("codeInputField");
            codeInputField.text = Xml.Trim();

            ScrollToTop();
        }

        void ScrollToTop()
        {
            // Scroll to the top
            var codeInputScrollView = xmlLayout.GetElementById<ScrollRect>("codeInputScrollView");
            codeInputScrollView.verticalNormalizedPosition = 1;
        }

        // Called by the codeInputField when its text changes
        void XmlChanged(string newXml)
        {
            Xml = newXml != null ? newXml.Trim() : "";
        }

        void ToggleViewportSize()
        {
            var outputPanel = xmlLayout.GetElementById("output");
            var expandedOutput = xmlLayout.GetElementById("expandedOutput");
            var mainPanel = xmlLayout.GetElementById("mainPanel");

            if (!viewPortExpanded)
            {
                expandedOutput.gameObject.SetActive(true);

                var expandedOutputPanel = xmlLayout.GetElementById("expandedOutputPanel");
                outputPanel.transform.SetParent(expandedOutputPanel.transform);

                mainPanel.gameObject.SetActive(false);
            }
            else
            {
                var outputContainer = xmlLayout.GetElementById("outputContainer");
                outputPanel.transform.SetParent(outputContainer.transform);

                expandedOutput.gameObject.SetActive(false);

                mainPanel.gameObject.SetActive(true);
            }

            viewPortExpanded = !viewPortExpanded;

            UpdateDisplay();
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            if (!Application.isPlaying) return;

            if (parseResult == ParseXmlResult.Changed)
            {
                UpdateDisplay();
            }
        }

        public void UpdateDisplay()
        {
            // Delaying until the end of the frame allows us to be certain that XmlLayout is completely set up before we execute this code
            XmlLayoutTimer.AtEndOfFrame(_UpdateDisplay, this);
        }

        void _UpdateDisplay()
        {
            var lineNumbers = xmlLayout.GetElementById<InputField>("lineNumbers");
            lineNumbers.text = String.Join( "\r\n", Enumerable.Range(1, 100).Select(i => i.ToString().PadLeft(2, '0')).ToArray());

            var outputField = xmlLayout.GetElementById("output");
            var outputFieldXmlLayout = outputField.gameObject.GetComponent<XmlLayout>() ?? outputField.gameObject.AddComponent<XmlLayout>();

            outputField.ApplyAttributes(GetOutputFieldAttributes());
            outputFieldXmlLayout.gameObject.SetActive(true);

            outputFieldXmlLayout.Hide(() =>
            {
                outputFieldXmlLayout.gameObject.SetActive(true);

                // borrow styles from the parent
                outputFieldXmlLayout.DefaultsFiles = this.xmlLayout.DefaultsFiles;

                outputFieldXmlLayout.Xml = this.Xml;
                try
                {
                    // We're using a custom log handler here so that any log/error messsages can be displayed in our message dialog
                    var oldHandler = Debug.unityLogger.logHandler;
                    Debug.unityLogger.logHandler = new TestLogHandler(MessageDialog, oldHandler);

                    outputFieldXmlLayout.RebuildLayout(false, true);

                    Debug.unityLogger.logHandler = oldHandler;
                }
                catch (Exception e)
                {
                    MessageDialog.Show("Xml Parse Error", e.Message);
                }

                outputField.ApplyAttributes(GetOutputFieldAttributes());
                outputFieldXmlLayout.Show();
            });
        }

        private AttributeDictionary GetOutputFieldAttributes()
        {
            // Essentially what we're doing here is overriding the default attributes
            // so as to animate out Output XmlLayout (even though the Xml code doesn't specify it)
            return new AttributeDictionary()
                    {
                        {"ShowAnimation", "Grow"},
                        {"HideAnimation", "FadeOut"},
                        {"AnimationDuration", "0.2"},
                    };
        }

        void Empty()
        {
            Xml = @"
<XmlLayout>
    <Include path=""Xml/Styles.xml"" />


</XmlLayout>
            ";

            UpdateCodeInputField();
            UpdateDisplay();
        }

        void ExampleA()
        {
            Xml = @"
<XmlLayout>
    <Defaults>
        <Text alignment=""MiddleCenter""
              fontStyle=""Bold""
              fontSize=""18""
              color=""white"" />

        <Text class=""header""
              color=""#00FF00""
              fontSize=""24""
              outline=""black"" />

        <Image preserveAspect=""true"" />
    </Defaults>

    <TableLayout cellPadding=""10"" cellSpacing=""5"">
        <Row preferredHeight=""48"">
            <Cell columnSpan=""3"">
                <Text class=""header"">Gems</Text>
            </Cell>
        </Row>
        <Row>
            <Cell>
                <Image image=""Sprites/Shop/gemRed"" />
            </Cell>
            <Cell>
                <Image image=""Sprites/Shop/gemBlue"" />
            </Cell>
            <Cell>
                <Image image=""Sprites/Shop/gemGreen"" />
            </Cell>
        </Row>
        <Row preferredHeight=""48"">
            <Cell><Text>Red</Text></Cell>
            <Cell><Text>Blue</Text></Cell>
            <Cell><Text>Green</Text></Cell>
        </Row>
    </TableLayout>
</XmlLayout>
            ";

            UpdateCodeInputField();
            UpdateDisplay();
        }

        void ExampleB()
        {
            Xml = @"
<XmlLayout>
    <Include path=""Xml/Styles.xml"" />

    <VerticalLayout padding=""20"" spacing=""5"">
        <Button>Button 1</Button>
        <Button>Button 2</Button>
        <Button>Button 3</Button>
        <Button>Button 4</Button>
        <Button>Button 5</Button>
        <Button>Button 6</Button>
        <Button>Button 7</Button>
        <Button>Button 8</Button>
    </VerticalLayout>
</XmlLayout>
            ";

            UpdateCodeInputField();
            UpdateDisplay();
        }

        void ExampleC()
        {
            Xml = @"
<XmlLayout>
    <Include path=""Xml/Styles.xml"" />

    <Defaults>
        <Panel class=""cornerPanel""
               width=""100""
               height=""50""
               color=""rgba(0,0.5,0,0.5)""
               image=""Sprites/Outline_With_Background""
        />

        <Text color=""#00FF00""
              fontStyle=""Bold""
              alignment=""MiddleCenter"" />
    </Defaults>

    <Panel width=""90%""
           height=""90%""
           image=""Sprites/Outline""
           color=""rgb(0.5,0.5,0.5)"">
        <Panel class=""cornerPanel""
               rectAlignment=""UpperLeft"">
            <Text>Upper Left</Text>
        </Panel>

        <Panel class=""cornerPanel""
               rectAlignment=""UpperRight"">
            <Text>Upper Right</Text>
        </Panel>

        <Image image=""Sprites/Shop/coin""
               width=""100""
               height=""100""
               rectAlignment=""MiddleCenter""
               preserveAspect=""true""
               allowDragging=""true""
               restrictDraggingToParentBounds=""false"" />

        <Text offsetXY=""0,-48"" raycastTarget=""false"">Try dragging the coin!</Text>

        <Panel class=""cornerPanel""
               rectAlignment=""LowerLeft"">
            <Text>Lower Left</Text>
        </Panel>

        <Panel class=""cornerPanel""
               rectAlignment=""LowerRight"">
            <Text>Lower Right</Text>
        </Panel>
    </Panel>

</XmlLayout>
            ";

            UpdateCodeInputField();
            UpdateDisplay();
        }
    }

    class TestLogHandler : ILogHandler
    {
        XmlLayout_Example_MessageDialog m_MessageDialog;
        ILogHandler m_OriginalLogger;

        public TestLogHandler(XmlLayout_Example_MessageDialog messageDialog, ILogHandler originalLogger)
        {
            m_MessageDialog = messageDialog;
            m_OriginalLogger = originalLogger;
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            // Pass on the message to the original logger so that it can be displayed on the console
            m_OriginalLogger.LogException(exception, context);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (!m_MessageDialog.gameObject.activeInHierarchy)
            {
                m_MessageDialog.Show(logType.ToString(), String.Format(format, args));
            }
            else
            {
                m_MessageDialog.AppendText(String.Format(format, args));
            }

            // Pass on the message to the original logger so that it can be displayed on the console as well
            m_OriginalLogger.LogFormat(logType, context, format, args);
        }
    }
}
