using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;
using static Dynamit.Operators;

namespace Dynamit
{
    public class Conditions : Dictionary<Tuple<string, Operators>, object>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key to check against</param>
        /// <param name="op">An equality operator, either '=' or '!='</param>
        /// <returns>The value for the key/operator pair</returns>
        public object this[string key, Operators op]
        {
            get { return this[new Tuple<string, Operators>(key, op)]; }
            set { this[new Tuple<string, Operators>(key, op)] = value; }
        }

        internal void ForEach(Action<KeyValuePair<Tuple<string, Operators>, object>, int> action)
        {
            var i = 0;
            foreach (var e in this) action(e, i++);
        }
    }

    public enum Operators
    {
        EQUALS,
        NOT_EQUALS
    }

    public static class Finder<T> where T : DDictionary
    {
        private static IEnumerable<DDictionary> EqualitySQL(KeyValuePair<Tuple<string, Operators>, object> c, string kvp)
        {
            var opString = c.Key.Item2 == EQUALS ? "=?" : "<>?";
            var SQL = $"SELECT t.Dictionary FROM {kvp} t WHERE t.Key =? AND t.ValueHash {opString}";
            return Db.SQL<DDictionary>(SQL, c.Key.Item1, c.Value.GetHashCode());
        }

        private static IEnumerable<T> All => Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t");

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalityConditions"></param>
        /// <returns></returns>
        public static IEnumerable<T> Where(Conditions equalityConditions = null)
        {
            var kvpTable = typeof(T).GetAttribute<DDictionaryAttribute>()?.KeyValuePairTable.FullName;
            if (!equalityConditions?.Any() != true) return All;
            var results = new HashSet<DDictionary>();
            equalityConditions.ForEach((cond, index) =>
            {
                if (index == 0) results.UnionWith(EqualitySQL(cond, kvpTable));
                else results.IntersectWith(EqualitySQL(cond, kvpTable));
            });
            return results.Cast<T>();
        }
    }
}