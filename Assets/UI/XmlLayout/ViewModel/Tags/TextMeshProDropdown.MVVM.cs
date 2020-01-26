#if TEXTMESHPRO_PRESENT && !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Xml.Tags
{
    public partial class TextMeshProDropdownTagHandler : ElementTagHandler, IHasXmlFormValue
    {
        protected override bool dontCallHandleDataSourceAttributeAutomatically { get { return true; } }

        public override void SetListData(IObservableList list)
        {            
            var options = (List<string>)list;

            if (options == null) Debug.LogWarning("[XmlLayout][MVVM][Dropdown] Warning: list provided for options needs to be a list of of string values.");

            var dropdown = ((TMP_Dropdown)primaryComponent);
            dropdown.options = options.Select(s => new TMP_Dropdown.OptionData(s)).ToList();
            dropdown.RefreshShownValue();
        }

        protected override void HandleDataSourceAttribute(string dataSource, string additionalDataSource = null)
        {
            var dataSourceObject = new XmlLayoutDropdownDataSource(dataSource, currentXmlElement, additionalDataSource);

            // remove any pre-existing entries (as the dataSource string may have changed)            
            currentXmlLayoutInstance.ElementDataSources.RemoveAll(ed => ed.XmlElement == currentXmlElement);
            currentXmlLayoutInstance.ElementDataSources.Add(dataSourceObject);

            if (dataSourceObject.BindingType == ViewModelBindingType.TwoWay)
            {
                var dropdown = (TMP_Dropdown)primaryComponent;
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
}
#endif
