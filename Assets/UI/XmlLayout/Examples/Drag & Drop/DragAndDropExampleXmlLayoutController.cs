using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;
using UI.Tables;
using UnityEngine.UI;

class DragAndDropExampleXmlLayoutController : XmlLayoutController
{    
    void ItemDropped(XmlElement droppedItem, XmlElement cell)
    {
        if (!cell.HasClass("itemCell") || !droppedItem.HasClass("item")) return;

        droppedItem.parentElement.RemoveChildElement(droppedItem);
        
        cell.AddChildElement(droppedItem);
        
        // debug text
        var tableCell = cell.GetComponent<TableCell>();               

        var debugText = xmlLayout.GetElementById<Text>("debugText");
        debugText.text = String.Format("Item '{0}' dropped on cell '{1}' in table '{2}'", droppedItem.name, GetCellPositionString(tableCell), GetTableName(tableCell));
    }

    string GetCellPositionString(TableCell cell)
    {
        var row = cell.GetRow();
        var table = row.GetTable();

        var rowPosition = table.Rows.IndexOf(row) + 1;
        var columnPosition = row.Cells.IndexOf(cell) + 1;

        return String.Format("{0},{1}", rowPosition, columnPosition);
    }

    string GetTableName(TableCell cell)
    {
        return cell.GetRow().GetTable().name;
    }

    void ReturnToMainExamples()
    {
        xmlLayout.Hide(() => { UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ExampleScene"); });        
    }

    void Awake()
    {
        xmlLayout.Show();
    }
}
