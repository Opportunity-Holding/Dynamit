using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Tries to create an index in the database.
        /// </summary>
        /// <param name="columns">The names of the columns included in the index</param>
        public static void CreateIndex(System.Type table, params string[] columns)
        {
            var nameHead = "DYNAMIT_GENERATED_INDEX_FOR_" + table.FullName.Replace('.', '_');
            var nameTail = string.Join("_", columns);
            try
            {
                var str = $"CREATE INDEX {nameHead}__{nameTail} ON {table.FullName} ({string.Join(",", columns)})";
                Db.SQL(str);
            }
            catch
            {
            }
        }
    }
}