using System;
using System.Collections.Generic;
using System.Linq;

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
        public static IEnumerable<T> DDictionary<T>(IDictionary<string, object> equalsConditions)
            where T : DDictionary
        {
            return DB.All<T>().Where(dict => equalsConditions.All(cond =>
            {
                object value;
                return dict.TryGetValue(cond.Key, out value) && value.Equals(cond.Value);
            }));
        }

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ANY of the
        /// provided predicates are true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public static IEnumerable<T> DDictionary<T>(params Predicate<DDictionary>[] predicates)
            where T : DDictionary
        {
            if (!predicates.Any()) return DB.All<T>();
            return DB.All<T>().Where(dict => predicates.Any(pred => pred(dict)));
        }
    }
}