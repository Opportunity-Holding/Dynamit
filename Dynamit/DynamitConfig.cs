using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starcounter;

namespace Dynamit
{
    public static class DynamitConfig
    {
        /// <summary>
        /// Sets up indexes and callbacks for Dynamit database classes to 
        /// improve runtime performance.
        /// </summary>
        /// <param name="setupIndexes"></param>
        public static void Init(bool setupIndexes = true)
        {
            var dicts = typeof(DDictionary).GetConcreteSubclasses();
            var dictsWithMissingAttribute = dicts.Where(d => d.GetAttribute<DDictionaryAttribute>() == null).ToList();
            if (dictsWithMissingAttribute.Any())
                throw new DDictionaryException(dictsWithMissingAttribute.First());

            //var lists = typeof(DList).GetConcreteSubclasses();
            //var listsWithMissingAttribute = lists.Where(d => d.GetAttribute<DListAttribute>() == null).ToList();
            //if (listsWithMissingAttribute.Any())
            //    throw new DListException(listsWithMissingAttribute.First());

            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            foreach (var pair in DB.All<DKeyValuePair>())
                Db.TransactAsync(() => pair.ValueHash = pair.Value.GetHashCode());

            if (!setupIndexes) return;
            foreach (var kvp in pairs)
            {
                try
                {
                    DB.CreateIndex(kvp, "Dictionary");
                }
                catch
                {
                }
                try
                {
                    DB.CreateIndex(kvp, "Dictionary", "Key");
                }
                catch
                {
                }
                try
                {
                    DB.CreateIndex(kvp, "Key", "ValueHash");
                }
                catch
                {
                }
            }
        }
    }
}