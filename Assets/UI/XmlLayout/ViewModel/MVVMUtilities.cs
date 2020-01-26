#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UI.Xml.Tags;

namespace UI.Xml
{
    public static class MVVMUtilities
    {
        /// <summary>
        /// Get the value of a nested member, e.g.
        /// ParentClass.ChildClass.childProperty
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object GetNestedMemberValue(Type type, string path, object o)
        {
            MemberInfo member = null;

            foreach (string memberName in path.Split('.'))
            {
                member = type.GetMember(memberName)[0];
                type = member.GetMemberDataType();

                o = member.GetMemberValue(o);
            }

            return o;
        }

        /// <summary>
        /// Set the value of a nested member, e.g.
        /// ParentClass.ChildClass.childProperty
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="o"></param>
        /// <param name="newValue"></param>
        public static void SetNestedMemberValue(Type type, string path, object o, object newValue, XmlLayout xmlLayout = null)
        {
            MemberInfo member = null;

            string[] parts = path.Split('.');

            for (int i = 0; i < parts.Length; ++i)
            {
                member = type.GetMember(parts[i])[0];
                type = member.GetMemberDataType();

                if (i != parts.Length - 1) o = member.GetMemberValue(o);
            }

            if (member != null)
            {
                if (newValue.GetType() == typeof(System.String) && newValue.GetType() != type)
                {
                    member.SetMemberValue(o, ((string)newValue).ChangeToType(type, xmlLayout));
                }
                else
                {
                    member.SetMemberValue(o, newValue);
                }
            }
        }
    }
}
#endif
