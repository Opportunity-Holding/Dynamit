using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starcounter;

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

        internal static T GetReference<T>(this ulong? objectNo) where T : class
        {
            return DbHelper.FromID(objectNo.GetValueOrDefault()) as T;
        }

        internal static TAttribute GetAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
        {
            return type?.GetCustomAttributes<TAttribute>().FirstOrDefault();
        }

        internal static bool HasAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
        {
            return (type?.GetCustomAttributes<TAttribute>().Any()).GetValueOrDefault();
        }

        internal static void CancelConstructor<T, TException>(this T obj, TException exception) where T : class where TException: Exception
        {
            try
            {
                obj?.Delete();
            }
            catch
            {
            }
            throw exception;
        }

        internal static bool IsNullable(this Type type, out Type baseType)
        {
            baseType = null;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
                return false;
            baseType = type.GenericTypeArguments[0];
            return true;
        }
    }
}