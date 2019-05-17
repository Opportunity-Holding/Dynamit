using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Starcounter;

namespace Dynamit
{
    /// <summary>
    /// Gets information about a DDictionary table
    /// </summary>
    public static class TableInfo<T> where T : class
    {
        /// <summary>
        /// The name of the key-value pair table
        /// </summary>
        public static string KvpTable
        {
            get
            {
                DynamitConfig.KvpMappings.TryGetValue(typeof(T).FullName ?? "", out var kvp);
                return kvp;
            }
        }
    }

    /// <summary>
    /// The main configuration class for Dynamit
    /// </summary>
    public static class DynamitConfig
    {
        internal static bool EscapeStrings;
        internal static IReadOnlyDictionary<string, string> KvpMappings { get; }
        internal static IList<Type> Dicts { get; }
        private static bool IsInitiated { get; set; }

        static DynamitConfig()
        {
            Dicts = typeof(DDictionary).GetConcreteSubclasses();
            var kvpMappings = new Dictionary<string, string>();
            foreach (var dict in Dicts)
                kvpMappings[dict.FullName ?? throw new Exception()] =
                    dict.GetInterface(typeof(IDDictionary<,>).FullName)?.GetGenericArguments()[1].FullName;
            KvpMappings = kvpMappings;
        }

        /// <summary>
        /// Sets up indexes and callbacks for Dynamit database classes to 
        /// improve runtime performance.
        /// </summary>
        /// <param name="setupIndexes"></param>
        /// <param name="enableEscapeStrings">If true, all strings surrounded with '\"' will be 
        /// escaped to ordinary strings. Necessary for some string casts to work properly with RESTar</param>
        public static void Init(bool setupIndexes = true, bool enableEscapeStrings = false)
        {
            if (IsInitiated) return;
            EscapeStrings = enableEscapeStrings;
            var dictsWithMissingInterface = Dicts.Where(d => d.GetInterface("IDDictionary`2") == null).ToList();
            if (dictsWithMissingInterface.Any())
                throw new MissingIDDictionaryException(dictsWithMissingInterface.First());
            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            var firstNested = pairs.FirstOrDefault(pair => pair.IsNested);
            if (firstNested != null)
                throw new NestedDKeyValuePairDeclarationException(firstNested);
            if (!setupIndexes)
            {
                IsInitiated = true;
                return;
            }

            foreach (var pair in pairs)
            {
                CreateIndex(pair, "Dictionary", "Key");
                CreateIndex(pair, "Key", "ValueTypeCode", "ValueHash");

                foreach (var old in Db.SQL<DKeyValuePair>($"SELECT t FROM {pair} t WHERE t.ValueTypeCode =?", 0))
                {
                    object value = old.Value;
                    var valueTypeCode = Type.GetTypeCode(value.GetType());
                    switch (valueTypeCode)
                    {
                        case TypeCode.String:
                        case TypeCode.Boolean:
                        case TypeCode.DateTime:
                            Db.TransactAsync(() => old.ValueTypeCode = valueTypeCode);
                            break;
                        default:
                            Db.TransactAsync(() => old.ValueTypeCode = TypeCode.Object);
                            break;
                    }
                }
            }
            IsInitiated = true;
        }

        internal static string Fnuttify(this string sqlKey) => $"\"{sqlKey.Replace(".", "\".\"")}\"";

        private static void CreateIndex(Type table, params string[] c)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            var indexName = $"DYNAMIT_GENERATED_INDEX_FOR_{table.FullName?.Replace('.', '_')}__{string.Join("_", c)}";
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name =?", indexName).FirstOrDefault() == null)
                Db.SQL($"CREATE INDEX {indexName} ON {table.FullName} ({string.Join(",", c.Select(Fnuttify))})");
        }
    }
}