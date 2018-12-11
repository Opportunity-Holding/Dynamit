using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;
using Starcounter.Nova;
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
            var dynamicEqualsNull = equalityConditions.Where(c =>
            {
                switch (c.key?.FirstOrDefault())
                {
                    case null:
                    case '\0': throw new Exception("Invalid Finder condition. Key cannot be null or empty");
                    case '$': return false;
                    default: return c.value == null && c.op == EQUALS;
                }
            }).ToArray();

            if (dynamicEqualsNull.Length == equalityConditions.Length)
            {
                var set = new HashSet<T>(All);
                foreach (var (key, _, _) in dynamicEqualsNull)
                    set.ExceptWith(Db.SQL<T>(WithKey, key));
                return set;
            }
            var first = true;
            var results = equalityConditions
                .GroupBy(cond => cond.key[0] == '$')
                .OrderByDescending(p => p.Key)
                .Aggregate(new HashSet<T>(), (set, group) =>
                {
                    void process(IEnumerable<T> image)
                    {
                        if (first)
                        {
                            set.UnionWith(image);
                            first = false;
                        }
                        else set.IntersectWith(image);
                    }

                    if (group.Key) // Key matches declared member
                    {
                        var scConditions = GetScConditions(group);
                        if (scConditions.ObjectNo.HasValue && scConditions.WhereString == null)
                            process(new[] {Db.FromId<T>(scConditions.ObjectNo.Value)});
                        else process(Db.SQL<T>($"{StaticSelect} WHERE {scConditions.WhereString}", scConditions.Values));
                    }
                    else // Key matches dynamic member
                    {
                        foreach (var (key, op, value) in group)
                        {
                            var valueTypeCode = Type.GetTypeCode(value?.GetType());
                            switch (valueTypeCode)
                            {
                                case TypeCode.Boolean:
                                case TypeCode.String:
                                case TypeCode.DateTime: break;
                                default:
                                    valueTypeCode = TypeCode.Object;
                                    break;
                            }
                            switch (value)
                            {
                                case null when op == NOT_EQUALS:
                                    process(Db.SQL<T>(WithKey, key));
                                    break;
                                case null: break;

                                case var other when op == NOT_EQUALS:
                                    var image = Db
                                        .SQL<T>(WithKeyAndNotEqualsValue, key, valueTypeCode, other.GetHashCode())
                                        .Union(All.Except(Db.SQL<T>(WithKey, key)));
                                    process(image);
                                    break;
                                case var other:
                                    process(Db.SQL<T>(WithKeyAndEqualsValue, key, valueTypeCode, other.GetHashCode()));
                                    break;
                            }
                        }
                    }
                    return set;
                });

            if (dynamicEqualsNull.Length == 0)
                return results;
            foreach (var (key, _, _) in dynamicEqualsNull)
                results.ExceptWith(Db.SQL<T>(WithKey, key));
            return results;
        }

        private static readonly string StaticSelect = $"SELECT t FROM {typeof(T).FullName.Fnuttify()} t";

        private static readonly string WithKey = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName.Fnuttify()}) " +
                                                 $"FROM {TableInfo<T>.KvpTable} t " +
                                                 "WHERE t.Key =?";

        private static readonly string WithKeyAndNotEqualsValue = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName.Fnuttify()}) " +
                                                                  $"FROM {TableInfo<T>.KvpTable} t " +
                                                                  "WHERE t.Key =? AND (t.ValueTypeCode <>? OR t.ValueHash <>?)";

        private static readonly string WithKeyAndEqualsValue = $"SELECT CAST(t.Dictionary AS {typeof(T).FullName.Fnuttify()}) " +
                                                               $"FROM {TableInfo<T>.KvpTable} t " +
                                                               "WHERE t.Key =? AND t.ValueTypeCode =? AND t.ValueHash =?";

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