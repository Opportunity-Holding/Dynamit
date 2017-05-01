using System.Collections.Generic;
using System.Linq;
using System;
using Starcounter;

namespace Dynamit
{
    /// <summary>
    /// This class provides static methods for database queries in the DRTB system.
    /// </summary>
    internal static class DB
    {
        public static ICollection<T> All<T>() where T : class
        {
            return Db.SQL<T>($"SELECT t FROM {typeof(T).FullName} t").ToList();
        }

        private static string Fnuttify(this string sqlKey) => $"\"{sqlKey.Replace(".", "\".\"")}\"";

        /// <summary>
        /// Tries to create an index in the database.
        /// </summary>
        /// <param name="cols">The names of the columns included in the index</param>
        internal static void CreateIndex(Type table, params string[] cols)
        {
            var indexName = $"DYNAMIT_GENERATED_INDEX_FOR_{table.FullName.Replace('.', '_')}__{string.Join("_", cols)}";
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name =?", indexName).First == null)
                Db.SQL($"CREATE INDEX {indexName} ON {table.FullName} ({string.Join(",", cols.Select(Fnuttify))})");
        }
    }
}