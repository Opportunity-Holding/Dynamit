using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    [Database]
    public abstract class DDictionary : IDictionary<string, object>
    {
        [Transient]
        private Dictionary<string, object> _dict;

        public string KvpTable { get; }

        public bool IsUpdated { get; set; }

        private Dictionary<string, object> Dict
        {
            get
            {
                if (IsUpdated)
                {
                    var d = MakeDict();
                    Db.Transact(() => { IsUpdated = false; });
                    return _dict = d;
                }
                return _dict ?? (_dict = MakeDict());
            }
        }

        public DDictionary()
        {
            KvpTable = GetType().GetAttribute<DDictionaryAttribute>().KeyValuePairTable.FullName;
        }

        public IEnumerable<DKeyValuePair> KeyValuePairs =>
            Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t WHERE t.Dictionary =?", this);

        private Dictionary<string, object> MakeDict()
        {
            return KeyValuePairs.ToDictionary(
                pair => (string) pair.Key,
                (dynamic pair) => (object) pair.Value
            );
        }

        protected abstract DKeyValuePair NewKeyPair(DDictionary dict, string key, object value = null);

        public void Update()
        {
            Db.Transact(() => { IsUpdated = true; });
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (Dict.ContainsKey(item.Key))
                throw new ArgumentException($"Error: key '{item.Key}' already in dictionary");
            NewKeyPair(this, item.Key, item.Value);
            Update();
        }

        public void Clear()
        {
            foreach (var pair in KeyValuePairs)
            {
                pair.Clear();
                pair.Delete();
            }
            Update();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        public int Count => Dict.Count;

        public bool IsReadOnly => false;

        public bool ContainsKey(string key)
        {
            return Dict.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
        }

        public bool Remove(string key)
        {
            try
            {
                var obj = DB.Get<DKeyValuePair>("Dictionary", this, "Key", key);
                obj?.Delete();
                Update();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            return Dict.TryGetValue(key, out value);
        }

        public dynamic this[string key]
        {
            get { return Dict[key]; }
            set
            {
                if (!Dict.ContainsKey(key))
                    Add(key, value);
                else if (value is IDynamicMetaObjectProvider)
                {
                    ValueTypes valueType;
                    value = Helper.GetStaticType(value, out valueType);
                }
                var dbKvp = Db.SQL<DKeyValuePair>(
                    $"SELECT t FROM {KvpTable} t WHERE t.Dictionary =? AND t.Key =?", this, key
                ).First;
                if (dbKvp == null)
                    throw new Exception("Unexpected KeyValuePair exception. KeyValuePair was null.");
                dbKvp.Value = value;
            }
        }

        public dynamic SafeGet(string key)
        {
            if (ContainsKey(key))
                return this[key];
            return null;
        }

        public ICollection<string> Keys => Dict.Keys;

        public ICollection<object> Values => Dict.Values;
    }
}