using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Starcounter;

namespace Dynamit
{
    public static class Helper
    {
        public static IList<Type> GetConcreteSubclasses(this Type baseType)
        {
            return baseType.GetSubclasses().Where(type => !type.IsAbstract).ToList();
        }

        public static IEnumerable<Type> GetSubclasses(this Type baseType)
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(baseType)
                select type;
        }

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


        internal static object GetStaticType(dynamic value, out ValueTypes valueType)
        {
            Console.WriteLine($"BEGAN GETSTATICTYPE at {DateTime.Now:O}");

            valueType = ValueTypes.String;
            object o;
            try
            {
                DateTime d = value;
                o = d;
                valueType = ValueTypes.DateTime;
            }
            catch
            {
                try
                {
                    string s = value;
                    o = s;
                    valueType = ValueTypes.String;
                }
                catch
                {
                    try
                    {
                        bool b = value;
                        o = b;
                        valueType = ValueTypes.Bool;
                    }
                    catch
                    {
                        try
                        {
                            int i = value;
                            o = i;
                            valueType = ValueTypes.Int;
                        }
                        catch
                        {
                            try
                            {
                                long l = value;
                                o = l;
                                valueType = ValueTypes.Long;
                            }
                            catch
                            {
                                try
                                {
                                    decimal d = value;
                                    o = decimal.Round(d, 6);
                                    valueType = ValueTypes.Decimal;
                                }
                                catch
                                {
                                    throw new Exception("Illegal type: " + value.GetType());
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"DONE GETSTATICTYPE at {DateTime.Now:O}");

            return o;
        }
    }
}