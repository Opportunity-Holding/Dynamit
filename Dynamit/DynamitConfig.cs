using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starcounter;

namespace Dynamit
{
    public static class DynamitConfig
    {
        private static bool InitCompleted;

        /// <summary>
        /// Sets up indexes and callbacks for Dynamit database classes to 
        /// improve runtime performance.
        /// </summary>
        /// <param name="setupIndexes"></param>
        /// <param name="setupHooks"></param>
        public static void Init(bool setupIndexes = true, bool setupHooks = true)
        {
            if (InitCompleted) return;

            var dicts = typeof(DDictionary).GetConcreteSubclasses();
            var dictsWithMissingAttribute = dicts.Where(d => d.GetAttribute<DDictionaryAttribute>() == null).ToList();
            if (dictsWithMissingAttribute.Any())
                throw new DListException(dictsWithMissingAttribute.First());

            //var lists = typeof(DList).GetConcreteSubclasses();
            //var listsWithMissingAttribute = lists.Where(d => d.GetAttribute<DListAttribute>() == null).ToList();
            //if (listsWithMissingAttribute.Any())
            //    throw new DListException(listsWithMissingAttribute.First());

            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();
            
            foreach (var pair in DB.All<DKeyValuePair>())
                Db.TransactAsync(() => { pair.ValueHash = pair.Value.GetHashCode(); });

            if (setupIndexes)
            {
                foreach (var d in dicts)
                {
                    // Add indexes?
                }
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

            if (setupHooks)
            {
                foreach (var d in dicts)
                {
                    typeof(Janitor)
                        .GetMethod("AddDDictHook", BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(d)
                        .Invoke(null, null);
                }

                foreach (var kvp in pairs)
                {
                    typeof(Janitor)
                        .GetMethod("AddKvpHook", BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(kvp)
                        .Invoke(null, null);
                }
            }
            InitCompleted = true;
        }

        private static class Janitor
        {
            public static void AddKvpHook<T>() where T : DKeyValuePair
            {
                Hook<T>.BeforeDelete += (s, p) => Db.TransactAsync(p.Clear);
            }

            public static void AddDDictHook<T>() where T : DDictionary
            {
                Hook<T>.BeforeDelete += (s, d) =>
                {
                    foreach (var pair in d.KeyValuePairs)
                        Db.TransactAsync(() =>
                        {
                            pair.Clear();
                            pair.Delete();
                        });
                };
            }
        }
    }
}