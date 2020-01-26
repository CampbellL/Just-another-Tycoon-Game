using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Xml;
using UI.Xml.CustomAttributes;
using UI.Xml.Tags;
using UI.Xml.Configuration;

namespace UI.Xml
{
    public static class XmlLayoutAssemblyHelper
    {
        private static XmlLayoutConfiguration XmlLayoutConfiguration
        {
            get
            {
                if (m_XmlLayoutConfiguration == null) m_XmlLayoutConfiguration = XmlLayoutUtilities.XmlLayoutConfiguration;
                return m_XmlLayoutConfiguration;
            }
        }
        private static XmlLayoutConfiguration m_XmlLayoutConfiguration;

        private static List<string> m_AssemblyNames = new List<string>();
        private static List<string> additionalAssemblies = new List<string>();

        public static void Reset()
        {
            ClearAssemblyNames();
        }

        public static void AddAdditionalAssembly(string newAssembly)
        {
            additionalAssemblies.Add(newAssembly);
        }

        public static void ClearAssemblyNames()
        {
            m_AssemblyNames.Clear();
        }

        public static List<string> GetAssemblyNames()
        {
            if (m_AssemblyNames.Count == 0)
            {
                if (XmlLayoutConfiguration == null) return new List<string>();

                m_AssemblyNames = new List<string>();

                m_AssemblyNames.Add(Assembly.GetExecutingAssembly().FullName);
                m_AssemblyNames.Add(Assembly.GetAssembly(typeof(ElementTagHandler)).FullName);

                if (XmlLayoutConfiguration.ComprehensiveCustomElementAndAttributeCheck)
                {
                    // Get all referenced assemblies
                    m_AssemblyNames.AddRange(Assembly.GetExecutingAssembly()
                                                     .GetReferencedAssemblies()
                                                     .Select(a => a.FullName)
                                                     .ToList());

                    // Get ALL assemblies...
                    m_AssemblyNames.AddRange(AppDomain.CurrentDomain
                                                      .GetAssemblies()
                                                      .Select(a => a.FullName));
                }

                if (XmlLayoutConfiguration.CustomAssemblyList != null) m_AssemblyNames.AddRange(XmlLayoutConfiguration.CustomAssemblyList);

                // filter out any duplicates
                m_AssemblyNames = m_AssemblyNames.Distinct().ToList();

                // These aren't true assemblies, they are dynamically generated and will fail to load if we try to use them
                m_AssemblyNames.RemoveAll(s => s.StartsWith("Anonymously Hosted DynamicMethods"));
                m_AssemblyNames.RemoveAll(s => s.StartsWith("Microsoft.GeneratedCode"));

                if (XmlLayoutConfiguration.AssemblyExcludeList != null)
                {
                    foreach (var exclude in XmlLayoutConfiguration.AssemblyExcludeList)
                    {
                        m_AssemblyNames.RemoveAll(s => s.StartsWith(exclude));
                    }
                }
            }

            m_AssemblyNames.AddRange(additionalAssemblies);
            m_AssemblyNames = m_AssemblyNames.Distinct().ToList();

            return m_AssemblyNames;
        }
    }
}
