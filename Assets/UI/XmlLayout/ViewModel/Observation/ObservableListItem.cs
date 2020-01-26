#if !ENABLE_IL2CPP && MVVM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Reflection;

namespace UI.Xml
{
    public class ObservableListItem : MarshalByRefObject
    {
        private static Dictionary<Type, List<MemberInfo>> _members = new Dictionary<Type, List<MemberInfo>>();
        private static List<MemberInfo> GetMembers(Type type)
        {
            if (!_members.ContainsKey(type))
            {
                _members.Add(type, type.GetMembers()
                                       .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                                       .ToList());
            }

            return _members[type];
        }

        private string _guid = null;
        private string guid
        {
            get
            {
                if (_guid == null) _guid = Guid.NewGuid().ToString();
                return _guid;
            }
        }


        public override bool Equals(object obj)
        {
            if (this.GetHashCode() == obj.GetHashCode()) return true;

            var objListItem = (ObservableListItem)obj;
            if (objListItem == null) return false;

            return this.guid == objListItem.guid;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
#endif
