using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Reflection;

namespace UI.Xml
{
    public static class ReflectionExtensions
    {
        private static Dictionary<MemberInfo, Type> memberTypeCache = new Dictionary<MemberInfo, Type>();

        /// <summary>
        /// Get the data type of a member, regardless of whether it is a field or a property
        /// (cached)
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Type GetMemberDataType(this MemberInfo memberInfo)
        {
            if (memberTypeCache.ContainsKey(memberInfo)) return memberTypeCache[memberInfo];

            var fieldInfo = memberInfo.DeclaringType.GetField(memberInfo.Name);
            if (fieldInfo != null)
            {
                memberTypeCache.Add(memberInfo, fieldInfo.FieldType);
                return fieldInfo.FieldType;
            }

            var propertyInfo = memberInfo.DeclaringType.GetProperty(memberInfo.Name);
            if (propertyInfo != null)
            {
                memberTypeCache.Add(memberInfo, propertyInfo.PropertyType);
                return propertyInfo.PropertyType;
            }

            return null;
        }

        /// <summary>
        /// Set the value for a member, regardless of whether it is a field or a property
        /// </summary>
        /// <param name="memberInfo"></param>
        public static void SetMemberValue(this MemberInfo memberInfo, object o, object newValue)
        {
            var fieldInfo = memberInfo.DeclaringType.GetField(memberInfo.Name);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(o, newValue);
                return;
            }

            var propertyInfo = memberInfo.DeclaringType.GetProperty(memberInfo.Name);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(o, newValue, null);
                return;
            }

            Debug.LogWarningFormat("[XmlLayout][Warning] Member '{0}' not found for SetValue().", memberInfo.Name);
        }

        /// <summary>
        /// Get a value for a member, regardless of whether it is a field or a property
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object GetMemberValue(this MemberInfo memberInfo, object o)
        {
            var fieldInfo = memberInfo.DeclaringType.GetField(memberInfo.Name);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(o);
            }

            var propertyInfo = memberInfo.DeclaringType.GetProperty(memberInfo.Name);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(o, XmlLayoutUtilities.BindingFlags, null, null, null);
            }

            Debug.LogWarningFormat("[XmlLayout][Warning] Member '{0}' not found for GetValue().", memberInfo.Name);
            return null;
        }

        /// <summary>
        /// Thanks to Nawfal for this method!
        /// http://stackoverflow.com/questions/2210309/how-to-find-out-if-a-property-is-an-auto-implemented-property-with-reflection
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool IsAutoProperty(this PropertyInfo prop)
        {
            if (!prop.CanWrite || !prop.CanRead)
                return false;

            return prop.DeclaringType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                     .Any(f => f.Name.Contains("<" + prop.Name + ">"));
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
