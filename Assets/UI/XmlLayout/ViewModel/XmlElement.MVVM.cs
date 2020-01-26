#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public partial class XmlElement
    {
        [XmlFieldName("vm-DataSource")]
        public string DataSource;

        /// <summary>
        /// Used by MVVM functionality to load data from an ObservableList.
        /// </summary>
        /// <param name="listData"></param>
        public void SetListData(IObservableList listData)
        {
            tagHandler.SetInstance(rectTransform, xmlLayout);
            tagHandler.SetListData(listData);
        }
    }
}
#endif
