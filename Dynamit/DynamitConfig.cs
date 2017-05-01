using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starcounter;

namespace Dynamit
{
    public static class DynamitConfig
    {
        internal static bool EscapeStrings;

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

            var dicts = typeof(DDictionary).GetConcreteSubclasses();
            var dictsWithMissingAttribute = dicts.Where(d => d.GetAttribute<DDictionaryAttribute>() == null).ToList();
            if (dictsWithMissingAttribute.Any())
                throw new DDictionaryException(dictsWithMissingAttribute.First());
            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            foreach (var pair in DB.All<DKeyValuePair>())
                Db.TransactAsync(() => pair.ValueHash = pair.Value?.GetHashCode());

            var lists = typeof(DList).GetConcreteSubclasses();
            var listsWithMissingAttribute = lists.Where(d => d.GetAttribute<DListAttribute>() == null).ToList();
            if (listsWithMissingAttribute.Any())
                throw new DListException(listsWithMissingAttribute.First());
            var elements = typeof(DElement).GetConcreteSubclasses();
            foreach (var pair in DB.All<DElement>())
                Db.TransactAsync(() => pair.ValueHash = pair.Value?.GetHashCode());

            if (!setupIndexes) return;
            foreach (var kvp in pairs)
            {
                DB.CreateIndex(kvp, "Dictionary");
                DB.CreateIndex(kvp, "Dictionary", "Key");
                DB.CreateIndex(kvp, "Key", "ValueHash");
            }
            foreach (var element in elements)
            {
                DB.CreateIndex(element, "List");
                DB.CreateIndex(element, "List", "Index");
                DB.CreateIndex(element, "List", "ValueHash");
            }
        }
    }
}