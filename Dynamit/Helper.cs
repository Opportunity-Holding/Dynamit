using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starcounter.Nova;

namespace Dynamit
{
    internal static class Helper
    {
        internal static IList<Type> GetConcreteSubclasses(this Type baseType)
        {
            return baseType.GetSubclasses().Where(type => !type.IsAbstract).ToList();
        }

        private static IEnumerable<Type> GetSubclasses(this Type baseType) =>
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.IsSubclassOf(baseType)
            select type;

        internal static TAttribute GetAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
        {
            return type?.GetCustomAttributes<TAttribute>().FirstOrDefault();
        }

        internal static ulong GetOid(this object obj) => Db.GetOid(obj);
    }
}