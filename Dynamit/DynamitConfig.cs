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
        /// <param name="setupHooks"></param>
        public static void Init(bool setupIndexes = true, bool setupHooks = true)
        {
            var dicts = typeof(DDictionary).GetConcreteSubclasses();
            var dictsWithMissingAttribute = dicts.Where(d => d.GetAttribute<DDictionaryAttribute>() == null).ToList();
            if (dictsWithMissingAttribute.Any())
                throw new ScDictionaryException(dictsWithMissingAttribute.First());

            var pairs = typeof(DKeyValuePair).GetConcreteSubclasses();

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
                        DB.CreateIndex(kvp,"Dictionary", "Key");
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
                        .GetMethod("AddScDictHook", BindingFlags.Public | BindingFlags.Static)
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
        }

        private static class Janitor
        {
            public static void AddKvpHook<T>() where T : DKeyValuePair
            {
                Hook<T>.BeforeDelete += (s, p) => Db.Transact(p.Clear);
            }

            public static void AddScDictHook<T>() where T : DDictionary
            {
                Hook<T>.BeforeDelete += (s, d) =>
                {
                    foreach (var pair in d.KeyValuePairs)
                        Db.Transact(() =>
                        {
                            pair.Clear();
                            pair.Delete();
                        });
                };
            }
        }
    }
}