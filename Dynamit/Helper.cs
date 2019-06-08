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

        private static IEnumerable<Type> GetSubclasses(this Type baseType) => AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            })
            .Where(type => type.IsSubclassOf(baseType));

        internal static TAttribute GetAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
        {
            return type?.GetCustomAttributes<TAttribute>().FirstOrDefault();
        }

        internal static ulong GetOid(this object obj) => Db.GetOid(obj);
    }
}