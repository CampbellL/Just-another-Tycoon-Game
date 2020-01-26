using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

namespace UI.Xml.Tags
{
#if !ENABLE_IL2CPP && MVVM_ENABLED
    public class DataTableTagHandler : ElementTagHandler, IObservableListTagHandler
#else
    public class DataTableTagHandler : ElementTagHandler
#endif
    {
        internal static XmlLayoutDataTable currentDataTable { get; private set; }

        /// <summary>
        /// A collection of DataTable elements present in the scene
        /// (used to quickly locate them for ViewModel updates)
        /// </summary>
        internal Dictionary<string, XmlLayoutDataTable> dataTableElements = new Dictionary<string, XmlLayoutDataTable>();        

        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<XmlLayoutDataTable>();
            }
        }

        public override bool isCustomElement { get { return true; } }
        public override string elementChildType { get { return "dataTable"; } }

        public override Dictionary<string, string> attributes
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"prettifyColumnHeaders", "xs:boolean"},
                };
            }
        }

#if !ENABLE_IL2CPP && MVVM_ENABLED
        protected override void HandleDataSourceAttribute(string dataSource, string additionalDataSource = null)
        {
            var dataSourceObject = new XmlElementDataSource(dataSource, currentXmlElement);

            // remove any pre-existing entries (as the dataSource string may have changed)
            currentXmlLayoutInstance.ElementDataSources.RemoveAll(ed => ed.XmlElement == currentXmlElement);
            currentXmlLayoutInstance.ElementDataSources.Add(dataSourceObject);

            // we won't be supporting 2 way binding for this just yet (this will be added in future)
        }


        public override bool UseParseChildElements { get { return true; } }

        public override void ParseChildElements(System.Xml.XmlNode xmlNode)
        {
            var dataSource = currentXmlElement.DataSource;

            if (String.IsNullOrEmpty(dataSource)) return;

            var xmlLayoutController = currentXmlLayoutInstance.XmlLayoutController;
            if (xmlLayoutController == null) return;

            var viewModelProperty = xmlLayoutController.GetType().GetProperty("viewModel");
            if (viewModelProperty == null)
            {
                Debug.LogWarning("[XmlLayout] Warning: Useage of the <List> element requires the XmlLayoutController to have a view model type defined.");
                return;
            }

            var viewModel = viewModelProperty.GetValue(xmlLayoutController, null);

            var listMember = viewModel.GetType().GetMember(dataSource).FirstOrDefault();

            if (listMember == null)
            {
                Debug.LogWarning("[XmlLayout] Warning: View Model does not contain a field or property for data source '" + dataSource + "'.");
                return;
            }

            IList list = (IList)listMember.GetMemberValue(viewModel);
            
            var observableList = (IObservableList)list;

            if (list == null || observableList == null)
            {
                // no data yet
                return;
            }

            var dataTableComponent = this.primaryComponent as XmlLayoutDataTable;
            string id = String.Empty;
            if(currentXmlElement.HasAttribute("id"))
            {
                id = currentXmlElement.attributes["id"];
            } 
            else 
            {
                id = observableList.guid;
                XmlLayoutTimer.AtEndOfFrame(() => { if (currentXmlElement != null) currentXmlElement.SetAndApplyAttribute("id", id); }, currentXmlElement, true);
            }

            if (dataTableElements.ContainsKey(id))
            {
                dataTableElements[id] = dataTableComponent;
            }
            else
            {
                dataTableElements.Add(id, dataTableComponent);
            }            
        }

        public override void SetListData(IObservableList list)
        {
            // I may want to move this to ParseChildElements

            if (list == null) return;            

            var items = list.GetItems();
            var dataTable = primaryComponent as XmlLayoutDataTable;            

            /*dataTable.SetData(items, list.GetItemType());*/

            var itemType = list.itemType;
            
            dataTable.InitMVVM(itemType, items);

            foreach (var item in items)
            {
                AddListItem(list, item, currentXmlElement.DataSource);
            }
        }

        public bool IsHandlingList(IObservableList list)
        {
            return dataTableElements.ContainsKey(list.guid);
        }

        public void RemoveListItem(IObservableList list, object item, string listName)
        {
            var dataTableElement = GetDataTable(list.guid);

            if (dataTableElement == null) return;

            dataTableElement.RemoveRowMVVM(list.GetGUID(item));
        }

        public void AddListItem(IObservableList list, object item, string listName)
        {
            var dataTableElement = GetDataTable(list.guid);

            if (dataTableElement == null) return;

            dataTableElement.AddRowMVVM(list, item, list.itemType);            
        }

        public void UpdateListItem(IObservableList list, int index, object item, string listName, string changedField = null)
        {
            //Debug.Log("DataTable::UpdateListItem:: " + listName + " :: " + changedField ?? "all");

            var dataTableElement = GetDataTable(list.guid);

            if (dataTableElement == null) return;

            dataTableElement.UpdateRowMVVM(list.GetGUID(item), item, changedField);
        }
#endif

        private XmlLayoutDataTable GetDataTable(string guid)
        {
            return dataTableElements.FirstOrDefault(dt => dt.Key == guid).Value;
        }

        public override void Open(AttributeDictionary attributes)
        {
            base.Open(attributes);

            currentDataTable = primaryComponent as XmlLayoutDataTable;
        }
    }
}
