using System;
using System.Collections.Generic;
using System.Linq;
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
            EscapeStrings = enableEscapeStrings;
            var dictsWithMissingInterface = Dicts.Where(d => d.GetInterface("IDDictionary`2") == null).ToList();
            if (dictsWithMissingInterface.Any())
                throw new MissingIDDictionaryException(dictsWithMissingInterface.First());
            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            var firstNested = pairs.FirstOrDefault(pair => pair.IsNested);
            if (firstNested != null)
                throw new NestedDKeyValuePairDeclarationException(firstNested);
            var lists = typeof(DList).GetConcreteSubclasses();
            var listsWithMissingAttribute = lists.Where(d => d.GetAttribute<DListAttribute>() == null).ToList();
            if (listsWithMissingAttribute.Any())
                throw new DListException(listsWithMissingAttribute.First());
            var elements = typeof(DElement).GetConcreteSubclasses();
            if (!setupIndexes) return;

            foreach (var kvp in pairs)
            {
                CreateIndex(kvp, "Dictionary");
                CreateIndex(kvp, "Dictionary", "Key");
                CreateIndex(kvp, "Key", "ValueHash");
            }
            foreach (var element in elements)
            {
                CreateIndex(element, "List");
                CreateIndex(element, "List", "Index");
                CreateIndex(element, "List", "ValueHash");
            }
        }

        private static string Fnuttify(this string sqlKey) => $"\"{sqlKey.Replace(".", "\".\"")}\"";

        private static void CreateIndex(Type table, params string[] c)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            var indexName = $"DYNAMIT_GENERATED_INDEX_FOR_{table.FullName?.Replace('.', '_')}__{string.Join("_", c)}";
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name =?", indexName).FirstOrDefault() == null)
                Db.SQL($"CREATE INDEX {indexName} ON {table.FullName} ({string.Join(",", c.Select(Fnuttify))})");
        }
    }
}