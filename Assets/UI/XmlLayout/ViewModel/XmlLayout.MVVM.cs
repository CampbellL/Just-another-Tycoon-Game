#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public partial class XmlLayout
    {
        public List<XmlElementDataSource> ElementDataSources = new List<XmlElementDataSource>();

        public List<XmlElement> GetElementsForDataSource(string dataSource, string additionalDataSource = null)
        {
            return ElementDataSources.Where(ed => ed.Matches(dataSource, additionalDataSource))
                                     .Where(ed => ed.XmlElement != null)
                                     .Select(ed => ed.XmlElement)
                                     .Distinct()
                                     .ToList();
        }
    }

    [Serializable]
    public class XmlElementDataSource
    {
        [SerializeField]
        public string DataSource;

        [SerializeField]
        public ViewModelBindingType BindingType;

        [SerializeField]
        public XmlElement XmlElement;

        public XmlElementDataSource()
        {
        }

        public XmlElementDataSource(string dataSource, XmlElement xmlElement)
        {
            var _dataSource = dataSource.StripChars('{', '}');

            this.DataSource = _dataSource.StripChars('?', '#');
            this.XmlElement = xmlElement;
            this.XmlElement.DataSource = this.DataSource;

            if (String.IsNullOrEmpty(this.DataSource)) return;

            if (_dataSource[0] == '?')
            {
                this.BindingType = ViewModelBindingType.OneWay;
            }
            else
            {
                this.BindingType = ViewModelBindingType.TwoWay;
            }
        }

        public virtual bool Matches(string dataSource, string additionalDataSource = null)
        {
            return DataSource == dataSource;
        }
    }
}
#endif
