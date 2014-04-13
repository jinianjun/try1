using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NHibernate.Proxy;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace AppBase.JSON
{
    public static class NHibernateJsonSerialize
    {

        public static string SerializeToJsonString<T>(this T itemToSerialize)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new NHibernateContractResolver(),

            };
            System.IO.StringWriter sw = new System.IO.StringWriter();
            try
            {
                serializer.Serialize(sw, itemToSerialize);
            }
            catch {
                throw;
            }
            return sw.GetStringBuilder().ToString();
        }

    }
    public class NHibernateContractResolver : DefaultContractResolver
    {
        private static readonly MemberInfo[] NHibernateProxyInterfaceMembers = typeof(INHibernateProxy).GetMembers();

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);
            members.RemoveAll(memberInfo => (IsMemberPartOfNHibernateProxyInterface(memberInfo))
                || (IsMemberDynamicProxyMixin(memberInfo)) || (IsMemberMarkedWithIgnoreAttribute(memberInfo, objectType))
                || (IsMemberInheritedFromProxySuperclass(memberInfo, objectType)) || (IsMemberInheritedFromDynamicProxy(memberInfo, objectType)));
            var actualMemberInfos = new List<MemberInfo>();
            foreach (var memberInfo in members)
            {
                var infos = memberInfo.DeclaringType.BaseType.GetMember(memberInfo.Name);
                actualMemberInfos.Add(infos.Length == 0 ? memberInfo : infos[0]);
            } 
            return actualMemberInfos;
        }

        private static bool IsMemberDynamicProxyMixin(MemberInfo memberInfo)
        {
            return memberInfo.Name == "__interceptors";
        }
        private static bool IsMemberInheritedFromProxySuperclass(MemberInfo memberInfo, Type objectType)
        {
            return memberInfo.DeclaringType.Assembly == typeof(INHibernateProxy).Assembly;
        }
        
        private static bool IsMemberInheritedFromDynamicProxy(MemberInfo memberInfo, Type objectType)
        {
            return memberInfo.DeclaringType.Assembly == typeof(Spring.Aop.Framework.DynamicProxy.AdvisedProxy).Assembly;
        }

        private static bool IsMemberMarkedWithIgnoreAttribute(MemberInfo memberInfo, Type objectType)
        {
            var infos = typeof(INHibernateProxy).IsAssignableFrom(objectType) ? objectType.BaseType.GetMember(memberInfo.Name) : objectType.GetMember(memberInfo.Name);
            return infos[0].GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0;
        }
        private static bool IsMemberPartOfNHibernateProxyInterface(MemberInfo memberInfo)
        {
            return Array.Exists(NHibernateProxyInterfaceMembers, mi => memberInfo.Name == mi.Name);
        }
    }

}
