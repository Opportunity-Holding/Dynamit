using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;
using static System.StringComparison;
using static Dynamit.Operator;

namespace Dynamit
{
    /// <summary>
    /// Provides get methods for DDictionary types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Finder<T> where T : DDictionary
    {
        /// <summary>
        /// Returns all entities of the given type
        /// </summary>
        public static IEnumerable<T> All => Db.SQL<T>($"SELECT t FROM {typeof(T).FullName.Fnuttify()} t");

        /// <summary>
        /// Returns the first DDictionary of the given derived type for which the
        /// provided equality condition is true. If no entity is found, returns null.
        /// </summary>
        public static T First(string key, Operator op, dynamic value) => First((key, op, value));

        /// <summary>
        /// Returns the first DDictionary of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns the
        /// first entity found. If no entity is found, returns null.
        /// </summary>
        public static T First(params (string key, Operator op, dynamic value)[] equalityConditions) => Where(equalityConditions)?.FirstOrDefault();

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which the provided equality condition is true. 
        /// </summary>
        public static IEnumerable<T> Where(string key, Operator op, object value) => Where((key, op, value));

        /// <summary>
        /// Returns all DDictionaries of the given derived type for which ALL of the
        /// provided equality conditions are true. If no conditions are given, returns all entities found. 
        /// </summary>
        public static IEnumerable<T> Where(params (string key, Operator op, object value)[] equalityConditions)
        {
            if (equalityConditions.Length == 0) return All;
            var results = new HashSet<T>();
            var first = true;

            void addToResults(IEnumerable<T> toAdd)
            {
                if (first)
                {
                    results.UnionWith(toAdd);
                    first = false;
                }
                else results.IntersectWith(toAdd);
            }

            var dynamicEqualsNulls = new List<(string key, Operator op, dynamic value)>();
            equalityConditions
                .GroupBy(cond => !string.IsNullOrWhiteSpace(cond.key)
                    ? cond.key[0] == '$'
                    : throw new Exception("Invalid Finder condition. Key cannot be null or empty"))
                .OrderByDescending(p => p.Key)
                .ForEach(group =>
                {
                    if (group.Key)
                    {
                        var scConditions = GetScConditions(group);
                        if (scConditions.ObjectNo.HasValue && scConditions.WhereString == null)
                            addToResults(new[] {Db.FromId<T>(scConditions.ObjectNo.Value)});
                        else addToResults(Db.SQL<T>(StaticSQL + scConditions.WhereString, scConditions.Values));
                    }
                    else
                        group.ForEach(cond =>
                        {
                            if (cond.value == null)
                            {
                                if (cond.op == EQUALS) dynamicEqualsNulls.Add(cond);
                                else addToResults(Db.SQL<T>($"{KVPSQL} IS NOT NULL", cond.key));
                            }
                            else addToResults(Db.SQL<T>($"{KVPSQL} {GetSql(cond.op)}?", cond.key, cond.value.GetHashCode()));
                        });
                });
            if (first) results.UnionWith(All);
            foreach (var (key, _, _) in dynamicEqualsNulls)
            {
                var initialCount = Db.SQL<long>(CountSQL, key).FirstOrDefault();
                if (initialCount > 0)
                    results.ExceptWith(Db.SQL<T>($"{KVPSQL} IS NOT NULL", key));
            }
            return results;
        }

        private static readonly string StaticSQL = $"SELECT t FROM {typeof(T).FullName.Fnuttify()} t WHERE ";

        private static readonly string KVPSQL = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName.Fnuttify()}) " +
                                                $"FROM {TableInfo<T>.KvpTable} t WHERE t.Key =? AND t.ValueHash";

        private static readonly string CountSQL = $"SELECT COUNT(t) FROM {TableInfo<T>.KvpTable.Fnuttify()} t WHERE t.Key =?";

        private static ScConditions GetScConditions(IEnumerable<(string key, Operator op, object value)> conds)
        {
            ulong? objectNo = null;
            var literals = new List<object>();
            var localLiterals = literals;
            var clause = string.Join(" AND ", conds.Select(c =>
            {
                var (key, op, value) = (c.key.Substring(1), GetSql(c.op), c.value);
                if (c.op == EQUALS)
                {
                    if (string.Equals("objectno", key, OrdinalIgnoreCase))
                    {
                        try
                        {
                            objectNo = (ulong) value;
                            return null;
                        }
                        catch
                        {
                            throw new Exception($"Invalid ObjectNo format. Should be positive integer, found {value ?? "null"}");
                        }
                    }
                    if (string.Equals("objectid", key, OrdinalIgnoreCase))
                    {
                        try
                        {
                            var objectID = (string) value;
                            objectNo = DbHelper.Base64DecodeObjectID(objectID);
                            return null;
                        }
                        catch
                        {
                            throw new Exception($"Invalid ObjectID format. Should be base64 string, found {value}");
                        }
                    }
                }
                if (value == null)
                {
                    switch (c.op)
                    {
                        case EQUALS: return $"t.{key.Fnuttify()} IS NULL";
                        case NOT_EQUALS: return $"t.{key.Fnuttify()} IS NOT NULL";
                    }
                }
                localLiterals.Add(c.value);
                return $"t.{key.Fnuttify()} {op} ? ";
            }).Where(p => p != null));
            if (clause == "")
            {
                clause = null;
                literals = null;
            }
            return new ScConditions(objectNo, clause, literals?.ToArray());
        }

        private static string GetSql(Operator op)
        {
            switch (op)
            {
                case EQUALS: return "=";
                case NOT_EQUALS: return "<>";
                default: throw new Exception($"Invalid operator in Finder condition. Expected 'EQUALS' or 'NOT_EQUALS', found '{op}'");
            }
        }
    }
}