using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    /// <summary>
    /// Provides get methods for DDictionary types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Finder<T> where T : DDictionary, IDDictionary<T, DKeyValuePair>
    {
        private static QueryResultRows<T> EqualitySQL((string key, Operator op, object value) cond, string kvp)
        {
            var SQL = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName}) " +
                      $"FROM {kvp} t WHERE t.Key =? AND t.ValueHash {cond.op.SQL}?";
            return Db.SQL<T>(SQL, cond.key, cond.value.GetHashCode());
        }

        /// <summary>
        /// Returns all entities of the given type
        /// </summary>
        public static QueryResultRows<T> All => Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t");

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns all entities found. 
        /// If no entities are found, returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalityConditions"></param>
        /// <returns></returns>
        public static IEnumerable<T> Where(params (string key, Operator op, dynamic value)[] equalityConditions)
        {
            var kvpTable = TableInfo<T>.KvpTable;
            if (equalityConditions?.Any() != true) return All;
            var results = new HashSet<T>();
            equalityConditions.ForEach((cond, index) =>
            {
                if (index == 0) results.UnionWith(EqualitySQL(cond, kvpTable));
                else results.IntersectWith(EqualitySQL(cond, kvpTable));
            });
            return results;
        }

        /// <summary>
        /// Returns the first DDictionary of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns the
        /// first entity found. If no entity is found, returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalityConditions"></param>
        public static T First(params (string key, Operator op, dynamic value)[] equalityConditions)
        {
            var kvpTable = TableInfo<T>.KvpTable;
            if (equalityConditions?.Any() != true) return All.FirstOrDefault();
            var results = new HashSet<T>();
            equalityConditions.ForEach((cond, index) =>
            {
                if (index == 0) results.UnionWith(EqualitySQL(cond, kvpTable));
                else results.IntersectWith(EqualitySQL(cond, kvpTable));
            });
            return results.FirstOrDefault();
        }
    }

    internal static class FinderExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> ienum, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ienum) action(e, i++);
        }

        internal static void ForEach<T>(this IEnumerable<T> ienum, Action<T> action)
        {
            foreach (var e in ienum) action(e);
        }
    }
}