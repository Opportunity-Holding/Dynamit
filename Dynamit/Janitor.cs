using Starcounter;

namespace Dynamit
{
    internal static class Janitor
    {
        public static void AddKvpHook<T>() where T : ScKeyValuePair
        {
            Hook<T>.BeforeDelete += (s, pair) => { Db.Transact(pair.Clear); };
        }

        public static void AddScDictHook<T>() where T : ScDictionary
        {
            Hook<T>.BeforeDelete += (s, dict) =>
            {
                foreach (var pair in dict.KeyValuePairs)
                    Db.Transact(() =>
                    {
                        pair.Clear();
                        pair.Delete();
                    });
            };
        }
    }
}