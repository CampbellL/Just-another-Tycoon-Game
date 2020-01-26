#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml.Tags
{
    public partial class DropdownTagHandler : ElementTagHandler, IHasXmlFormValue
    {
        protected override bool dontCallHandleDataSourceAttributeAutomatically { get { return true; } }

        public override void SetListData(IObservableList list)
        {
            var options = (List<string>)list;

            if (options == null) Debug.LogWarning("[XmlLayout][MVVM][Dropdown] Warning: list provided for options needs to be a list of of string values.");

            ((Dropdown)primaryComponent).SetOptions(options);
        }

        protected override void HandleDataSourceAttribute(string dataSource, string additionalDataSource = null)
        {
            var dataSourceObject = new XmlLayoutDropdownDataSource(dataSource, currentXmlElement, additionalDataSource);

            // remove any pre-existing entries (as the dataSource string may have changed)
            currentXmlLayoutInstance.ElementDataSources.RemoveAll(ed => ed.XmlElement == currentXmlElement);
            currentXmlLayoutInstance.ElementDataSources.Add(dataSourceObject);

            if (dataSourceObject.BindingType == ViewModelBindingType.TwoWay)
            {
                var dropdown = (Dropdown)primaryComponent;
                var controller = (XmlLayoutControllerMVVM)currentXmlLayoutInstance.XmlLayoutController;

                dropdown.onValueChanged.AddListener((i) =>
                {
                    var dataType = controller.GetViewModelMemberDataType(dataSource);
                    if (dataType == typeof(int))
                    {
                        controller.SetViewModelValue(dataSource, dropdown.value, true);
                    }
                    else if(dataType == typeof(string))
                    {
                        controller.SetViewModelValue(dataSource, dropdown.options[dropdown.value].text, true);
                    }
                });
            }
        }
    }

    public class XmlLayoutDropdownDataSource : XmlElementDataSource
    {
        [SerializeField]
        public string OptionsDataSource;

        public XmlLayoutDropdownDataSource(string dataSource, XmlElement xmlElement, string optionsDataSource)
            : base(dataSource, xmlElement)
        {
            OptionsDataSource = optionsDataSource;
        }

        public override bool Matches(string dataSource, string additionalDataSource = null)
        {
            return DataSource == dataSource || OptionsDataSource == additionalDataSource;
        }
    }
}
#endif
