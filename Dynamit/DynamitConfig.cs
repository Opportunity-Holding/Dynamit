using System.Reflection;

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
            var dicts = typeof(ScDictionary).GetConcreteSubclasses();
            var pairs = typeof(ScKeyValuePair).GetConcreteSubclasses();

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
                        DB.CreateIndex(kvp.FullName, "Dictionary");
                    }
                    catch
                    {
                    }
                    try
                    {
                        DB.CreateIndex(kvp.FullName, "Key", "Dictionary");
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
    }
}