using System;
using System.Collections.Generic;
using System.Linq;

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

        internal static object GetStaticType(dynamic value, out ValueTypes valueType)
        {
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
            return o;
        }
    }
}