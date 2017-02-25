using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    public static class Finder
    {
        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="equalsConditions"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(IDictionary<string, object> equalsConditions)
            where T : DDictionary
        {
            var kvpTable = typeof(T).GetAttribute<DDictionaryAttribute>()?.KeyValuePairTable.FullName;
            IEnumerable<T> matches = new HashSet<T>();
            var sqlstring = $"SELECT t.Dictionary FROM {kvpTable} t WHERE t.Key=? AND t.ValueHash=? ";
            if (equalsConditions.Any())
            {
                var first = true;
                foreach (var econd in equalsConditions)
                {
                    if (first)
                    {
                        ((HashSet<T>) matches).UnionWith(Db.SQL<T>(sqlstring, econd.Key,
                            econd.Value?.GetHashCode()));
                        first = false;
                    }
                    else
                        ((HashSet<T>) matches).IntersectWith(Db.SQL<T>(sqlstring, econd.Key,
                            econd.Value?.GetHashCode()));
                }
            }
            else matches = Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t");
            return matches;
        }

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ANY of the
        /// provided predicates are true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(params Predicate<T>[] predicates)
            where T : DDictionary
        {
            if (!predicates.Any()) return DB.All<T>();
            return DB.All<T>().Where(dict => predicates.Any(pred =>
            {
                try
                {
                    return pred(dict);
                }
                catch
                {
                    return false;
                }
            }));
        }
    }
}