#if !ENABLE_IL2CPP && MVVM_ENABLED
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Tables;

namespace UI.Xml
{    
    public partial class XmlLayoutDataTable : MonoBehaviour
    {               
        [SerializeField]
        internal List<XmlLayoutDataTableRow> mvvmRows = new List<XmlLayoutDataTableRow>();

        internal void InitMVVM(Type type, IList<object> initialRowData)
        {
            ClearData();
            mvvmRows.Clear();

            if(type == typeof(Dictionary<string, string>))
            {
                headings = ExtractHeadingsFromDictionary(initialRowData.Cast<Dictionary<string,string>>().ToList());
            } 
            else 
            {
                headings = ExtractHeadingsFromType(type);
            }
            
            RenderHeadingRow(headings);
        }        

        internal XmlLayoutDataTableRow AddRowMVVM(IObservableList list, object rowData, Type type)
        {                        
            TableRow row;

            if (type == typeof(Dictionary<string, string>))
            {
                row = RenderDataRow(rowData as Dictionary<string,string>);
            }
            else
            {
                row = RenderDataRow(ExtractRowData(rowData, type));
            }
            
            var tracker = row.gameObject.AddComponent<XmlLayoutDataTableRow>();
            tracker.guid = list.GetGUID(rowData);

            mvvmRows.Add(tracker);

            XmlLayoutTimer.AtEndOfFrame(table.CalculateLayoutInputHorizontal, this);

            return tracker;
        }

        internal void RemoveRowMVVM(string guid)
        {
            var row = mvvmRows.FirstOrDefault(r => r.guid == guid);
            
            if (row != null)
            {
                _Destroy(row);
            }

            XmlLayoutTimer.AtEndOfFrame(table.CalculateLayoutInputHorizontal, this);
        }

        internal void UpdateRowMVVM(string rowGuid, object rowData, string changedField = null)
        {
            var row = mvvmRows.Where(r => r != null).FirstOrDefault(r => r.guid == rowGuid);

            if (row != null)
            {
                var isDictionary = rowData.GetType() == typeof(Dictionary<string, string>);
                Dictionary<string, string> dictionary = null;
                if (isDictionary)
                {
                    dictionary = rowData as Dictionary<string, string>;
                }
                
                var rowIndex = mvvmRows.IndexOf(row);

                if (changedField != null)
                {
                    object value = null;

                    if (isDictionary)
                    {
                        value = dictionary.ContainsKey(changedField) ? dictionary[changedField] : null;
                    }
                    else
                    {
                        value = rowData.GetType().GetMember(changedField).First().GetMemberValue(rowData);
                    }

                    SetCellValue(rowIndex, changedField, value);
                }
                else
                {                    
                    foreach (var heading in headings)
                    {
                        object value = null;

                        if (isDictionary)
                        {
                            value = dictionary.ContainsKey(heading) ? dictionary[heading] : null;
                        }
                        else
                        {
                            value = rowData.GetType().GetMember(heading).First().GetMemberValue(rowData);
                        }

                        SetCellValue(rowIndex, heading, value);
                    }
                }                
            }
        }
    }
}
#endif
