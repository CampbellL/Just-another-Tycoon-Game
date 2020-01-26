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
    [ExecuteInEditMode]
    public partial class XmlLayoutDataTable : MonoBehaviour
    {
        #region Configuration
        [Header("Configuration")]
        public bool PrettifyColumnHeaders = true;
        #endregion

        #region Internal References
        [SerializeField, HideInInspector]
        internal TableRow headerRow;

        [SerializeField, HideInInspector]
        internal List<TableRow> dataRows = new List<TableRow>();

        [SerializeField, HideInInspector]
        protected List<string> headings = new List<string>();
        #endregion

        #region External References
        [Header("Object References")]
        public TableLayout table;

        public TableRow templateHeaderRow;
        public TableCell templateHeaderCell;

        public TableRow templateDataRow;
        public TableCell templateDataCell;
        #endregion

        /// <summary>
        /// Set the Data used by this XmlLayoutDataTable from a list of objects or List<Dictionary<string, string>>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columns">A Dictionary<string, string> specifying the column headings (key == actual heading, value == displayed heading)</param>
        /// <param name="dataSource"></param>
        public void SetData<T>(Dictionary<string, string> columns, List<T> dataSource)
        {
            ClearData();

            HandleColumnsDefinition(columns);

            if (dataSource == null) return;

            RenderDataRows(dataSource, typeof(T));
        }

        /// <summary>
        /// Set the Data used by this XmlLayoutDataTable from a list of objects or List<Dictionary<string, string>>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        public void SetData<T>(List<T> rows)
        {
            ClearData();

            if (rows == null) return;

            if (TryHandleDataSourceAsDictionary<T>(rows))
            {
                // If this is actually a Dictionary<string, string>, then TryHandleDataSourceAsDictionary will have handled this already
                return;
            }

            // extract the headings from the object's type
            var type = typeof(T);
            RenderFromList(rows.Cast<object>().ToList(), type);
        }

        /// <summary>
        /// Set the data used by this XmlLayoutTable from a list of objects or List<Dictionary<string, string>>
        /// (With type as a parameter instead of as a generic argument)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="itemType"></param>
        public void SetData(List<object> rows, Type itemType)
        {
            ClearData();

            if (rows == null) return;

            if (itemType == typeof(Dictionary<string, string>))
            {
                SetDataFromDictionary(rows.Cast<Dictionary<string, string>>().ToList());
                return;
            }

            RenderFromList(rows, itemType);
        }

        protected void RenderFromList(List<object> rows, Type itemType)
        {
            headings = ExtractHeadingsFromType(itemType);

            RenderHeadingRow(headings);

            if (rows.Any()) RenderDataRows(rows, itemType);
        }

        protected List<string> ExtractHeadingsFromType(Type type)
        {
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            return members.Select(m => m.Name).ToList();
        }

        protected void SetDataFromDictionary(List<Dictionary<string, string>> data)
        {
            //ClearData();

            headings = ExtractHeadingsFromDictionary(data);

            RenderHeadingRow(headings);
            RenderDataRows(data, typeof(Dictionary<string, string>));
        }

        public bool TryHandleDataSourceAsDictionary<T>(List<T> dataSource)
        {
            if (typeof(T) == typeof(Dictionary<string, string>))
            {
                SetDataFromDictionary(dataSource.Cast<Dictionary<string, string>>().ToList());
                return true;
            }

            return false;
        }

        protected void RenderDataRowsFromDictionary(List<Dictionary<string, string>> rows)
        {
            foreach (var row in rows)
            {
                RenderDataRow(row);
            }
        }

        protected void RenderDataRows<T>(List<T> rows, Type type)
        {
            if (type == typeof(Dictionary<string, string>))
            {
                RenderDataRowsFromDictionary(rows.Cast<Dictionary<string,string>>().ToList());
                return;
            }

            Dictionary<string, MemberInfo> members = GetMembers(type);

            List<Dictionary<string, string>> processedRowData = new List<Dictionary<string, string>>();
            foreach (var row in rows)
            {
                processedRowData.Add(ExtractRowData(row,  type, members));
            }

            RenderDataRowsFromDictionary(processedRowData);
        }


        private Dictionary<Type, Dictionary<string, MemberInfo>> cachedMembers = new Dictionary<Type, Dictionary<string, MemberInfo>>();

        private Dictionary<string, MemberInfo> GetMembers(Type type)
        {
            if (!cachedMembers.ContainsKey(type))
            {
                cachedMembers.Add(type, headings.ToDictionary(k => k, v => type.GetMember(v).FirstOrDefault())
                                                .Where(kvp => kvp.Value != null)
                                                .ToDictionary(k => k.Key, v => v.Value));
            }

            return cachedMembers[type];
        }

        protected Dictionary<string, string> ExtractRowData(object rowDataObject, Type type, Dictionary<string, MemberInfo> members = null)
        {
            if (members == null)
            {
                members = GetMembers(type);
            }

            var rowData = new Dictionary<string, string>();

            foreach (var heading in headings)
            {
                var member = members[heading];
                if (member != null)
                {
                    rowData.Add(heading, member != null ? member.GetMemberValue(rowDataObject).ToString() : String.Empty);
                }
            }

            return rowData;
        }

        private void HandleColumnsDefinition(Dictionary<string, string> columns)
        {
            headings = columns.Keys.ToList();

            RenderHeadingRow(columns.Values.ToList(), true);
        }

        protected TableRow RenderDataRow(Dictionary<string, string> data)
        {
            var dataRow = Instantiate<TableRow>(templateDataRow, "Data Row");
            table.AddRow(dataRow);

            // create the data cells
            foreach (var heading in headings)
            {
                var dataCell = Instantiate<TableCell>(templateDataCell, "Data Cell");
                dataRow.AddCell(dataCell);

                var text = dataCell.GetComponentInChildren<Text>();
                text.text = data.ContainsKey(heading) ? data[heading] : String.Empty;
            }

            dataRows.Add(dataRow);

            XmlLayoutTimer.AtEndOfFrame(() => { if (table != null) table.CalculateLayoutInputHorizontal(); }, this);

            return dataRow;
        }

        protected void RenderHeadingRow(List<string> headings, bool doNotPrettify = false)
        {
            var displayHeadings = headings;

            if (PrettifyColumnHeaders && !doNotPrettify)
            {
                displayHeadings = displayHeadings.Select(h => h.SplitByCapitals().ToTitleCase()).ToList();
            }

            // create the header row
            headerRow = Instantiate<TableRow>(templateHeaderRow, "Header Row");
            table.AddRow(headerRow);

            // create the header cells
            foreach (var heading in displayHeadings)
            {
                var headingCell = Instantiate<TableCell>(templateHeaderCell, "Header Cell");
                headerRow.AddCell(headingCell);

                var text = headingCell.GetComponentInChildren<Text>();
                text.text = heading;
            }
        }

        protected List<string> ExtractHeadingsFromDictionary(List<Dictionary<string, string>> data)
        {
            var headings = new List<string>();

            foreach (var dataRow in data)
            {
                foreach (var kvp in dataRow)
                {
                    if (!headings.Contains(kvp.Key)) headings.Add(kvp.Key);
                }
            }

            return headings;
        }

        /// <summary>
        /// Remove all visible rows, with the option to keep the heading row
        /// </summary>
        /// <param name="preserveHeadingRow"></param>
        public void ClearData(bool preserveHeadingRow = false)
        {
            if (!preserveHeadingRow && headerRow != null) _Destroy(headerRow);

            if (dataRows.Any())
            {
                dataRows.Where(dr => dr != null).ToList().ForEach(dr => _Destroy(dr));
                dataRows.Clear();
            }
        }

        private T Instantiate<T>(T template, string name = "")
            where T : UnityEngine.MonoBehaviour
        {
            var item = GameObject.Instantiate<T>(template);
            item.gameObject.SetActive(true);
            item.name = name;

            return item;
        }

        protected void _Destroy(UnityEngine.Object o)
        {
            if (o == null) return;

            if (o is MonoBehaviour) o = ((MonoBehaviour)o).gameObject;

            if (Application.isPlaying)
            {
                Destroy(o);
            }
            else
            {
                DestroyImmediate(o);
            }
        }

        public void SetCellValue(int rowIndex, string columnName, object value)
        {
            if (rowIndex > dataRows.Count)
            {
                Debug.LogWarningFormat("[XmlLayout][DataTable][SetCellValue]: Invalid rowIndex '{0}' provided.", rowIndex);
                return;
            }

            var row = dataRows[rowIndex];
            var columnIndex = headings.IndexOf(columnName);

            if (columnIndex == -1 || columnIndex > row.Cells.Count)
            {
                Debug.LogWarningFormat("[XmlLayout][DataTable][SetCellValue]: Invalid columnName '{0}' provided.", columnName);
                return;
            }

            var cell = row.Cells[columnIndex];

            // In future, this will be used to determine how to render the data, for now, it is just text
            var columnType = "text";
            _SetCellValue(cell, value, columnType);
        }

        private void _SetCellValue(TableCell cell, object value, string columnType)
        {
            switch (columnType)
            {
                case "text":
                    {
                        var text = cell.gameObject.GetComponentInChildren<Text>();
                        text.text = value.ToString();
                    }
                    break;
            }
        }
    }
}
