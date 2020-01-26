#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UI.Xml;

namespace UI.Xml.Examples
{
    [ExecuteInEditMode]
    partial class DataTableExampleController : XmlLayoutController<DataTableExampleViewModel>
    {
        protected override void PrepopulateViewModelData()
        {
            viewModel.myData = new ObservableList<DataTableExampleListItem>()
            {
                new DataTableExampleListItem("A", "B", "C", "D"),
                new DataTableExampleListItem("E", "F", "G", "H"),
                new DataTableExampleListItem("I", "J", "K", "L"),
                new DataTableExampleListItem("M", "N", "O", "P"),
                //new DataTableExampleListItem("Q", "R", "S", "T"),
                //new DataTableExampleListItem("U", "V", "X", "Y"),
            };

            viewModel.myData2 = new ObservableList<Dictionary<string, string>>()
            {
                new Dictionary<string, string>() { { "A", "1" }, {"B", "2"}, {"C", "3"} },
                new Dictionary<string, string>() { { "A", "4" }, {"B", "5"}, {"C", "6"} },
                new Dictionary<string, string>() { { "A", "7" }, {"B", "8"}, {"C", "9"} },
                new Dictionary<string, string>() { { "A", "10" }, {"B", "11"}, {"C", "12"} },
                //new Dictionary<string, string>() { { "A", "13" }, {"B", "14"}, {"C", "15"} },
                //new Dictionary<string, string>() { { "A", "16" }, {"B", "17"}, {"C", "18"} },
            };
        }

        #region MVVM1
        void MVVM1_AddItem()
        {
            viewModel.myData.Add(new DataTableExampleListItem("New", "New", "New", "New"));
        }

        void MVVM1_RemoveLast()
        {
            if (viewModel.myData.Any()) viewModel.myData.RemoveAt(viewModel.myData.Count - 1);
        }

        void MVVM1_ChangeLast()
        {
            if (viewModel.myData.Any()) viewModel.myData.Last().col3 = "+++";
        }

        void MVVM1_ReplaceLast()
        {
            if (viewModel.myData.Any()) viewModel.myData[viewModel.myData.Count - 1] = new DataTableExampleListItem("***", "***", "***", "***");
        }
        #endregion

        #region MVVM2
        void MVVM2_AddItem()
        {
            viewModel.myData2.Add(new Dictionary<string,string>() { { "A", "New" }, {"B", "New"}, {"C", "New"}, {"D", "New" } });
        }

        void MVVM2_RemoveLast()
        {
            if(viewModel.myData2.Any()) viewModel.myData2.Remove(viewModel.myData2.Last());
        }

        void MVVM2_ReplaceLast()
        {
            if (viewModel.myData2.Any()) viewModel.myData2[viewModel.myData2.Count - 1] = new Dictionary<string, string>() { { "A", "***" }, { "B", "***" }, { "C", "***" }, { "D", "***" } };
        }
        #endregion

    }
}
#endif
