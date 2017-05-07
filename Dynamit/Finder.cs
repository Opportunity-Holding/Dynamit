using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    public class Conditions : Dictionary<Tuple<string, string>, object>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key to check against</param>
        /// <param name="op">An equality operator, either '=' or '!='</param>
        /// <returns>The value for the key/operator pair</returns>
        public object this[string key, string op]
        {
            get { return this[new Tuple<string, string>(key, op)]; }
            set
            {
                switch (op)
                {
                    case "=":
                    case "!=": break;
                    default: throw new Exception($"Invalid operator {op}. Valid operators: \'=\', \'!=\'");
                }
                this[new Tuple<string, string>(key, op)] = value;
            }
        }

        internal void ForEach(Action<KeyValuePair<Tuple<string, string>, object>, int> action)
        {
            var i = 0;
            foreach (var e in this) action(e, i++);
        }
    }

    public static class Finder<T> where T : DDictionary
    {
        private static IEnumerable<DDictionary> EqualitySQL(KeyValuePair<Tuple<string, string>, object> c, string kvp)
        {
            var SQL = $"SELECT t.Dictionary FROM {kvp} t WHERE t.Key =? AND t.ValueHash {c.Key.Item2}?";
            return Db.SQL<DDictionary>(SQL, c.Key.Item1, c.Value.GetHashCode());
        }

        public static IEnumerable<T> All => Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t");

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalityConditions"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select(Conditions equalityConditions = null)
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