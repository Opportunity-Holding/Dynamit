using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Dynamit
{
    internal static class Helper
    {
        internal static IList<Type> GetConcreteSubclasses(this Type baseType)
        {
            return baseType.GetSubclasses().Where(type => !type.IsAbstract).ToList();
        }

        private static IEnumerable<Type> GetSubclasses(this Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            }).Where(type => type.IsSubclassOf(baseType));

//            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
//                from type in assembly.GetTypes()
//                where type.IsSubclassOf(baseType)
//                select type;
        }

        internal static TAttribute GetAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
        {
            return type?.GetCustomAttributes<TAttribute>().FirstOrDefault();
        }
    }
}