﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    [Database]
    public abstract class DDictionary : IDictionary<string, object>, IEntity
    {
        public string KvpTable { get; }

        public DDictionary()
        {
            KvpTable = GetType().GetAttribute<DDictionaryAttribute>().KeyValuePairTable.FullName;
        }

        public IEnumerable<DKeyValuePair> KeyValuePairs =>
            Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t WHERE t.Dictionary =?", this);

        private IEnumerable<KeyValuePair<string, object>> _keyValuePairs =>
            Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t WHERE t.Dictionary =?", this)
                .Select(p => new KeyValuePair<string, object>(p.Key, p.Value));

        protected abstract DKeyValuePair NewKeyPair(DDictionary dict, string key, object value = null);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _keyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (item.Value == null) return;
            if (ContainsKey(item.Key))
                throw new ArgumentException($"Error: key '{item.Key}' already in dictionary");
            NewKeyPair(this, item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var pair in KeyValuePairs)
            {
                pair.Clear();
                pair.Delete();
            }
        }

        public void ClearAndDelete()
        {
            Clear();
            this.Delete();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _keyValuePairs.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Count > array.Length - 1) throw new ArgumentException(nameof(arrayIndex));
            foreach (var kvp in KeyValuePairs)
            {
                array[arrayIndex] = kvp;
                arrayIndex += 1;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        public int Count => KeyValuePairs.Count();

        public bool IsReadOnly => false;

        public bool ContainsKey(string key)
        {
            return Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t " +
                                         "WHERE t.Dictionary =? AND t.Key =?", this, key).First != null;
        }

        public void Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
        }

        public bool Remove(string key)
        {
            try
            {
                var obj = Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t " +
                                                "WHERE t.Dictionary =? AND t.Key =?", this, key).First;
                obj?.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            var match = Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t " +
                                              "WHERE t.Dictionary =? AND t.Key =? ", this, key).First;
            value = match?.Value;
            return match != null;
        }

        public dynamic this[string key]
        {
            get
            {
                var match = Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t " +
                                                  "WHERE t.Dictionary =? AND t.Key =? ", this, key).First;
                if (match == null) throw new KeyNotFoundException();
                return match.Value;
            }
            set
            {
                if (value is IDynamicMetaObjectProvider)
                {
                    ValueTypes valueType;
                    value = Helper.GetStaticType(value, out valueType);
                }
                var dbKvp = Db.SQL<DKeyValuePair>(
                    $"SELECT t FROM {KvpTable} t WHERE t.Dictionary =? AND t.Key =?", this, key
                ).First;
                if (value == null)
                {
                    if (dbKvp == null) return;
                    dbKvp.Clear();
                    dbKvp.Delete();
                    return;
                }
                if (dbKvp == null)
                    NewKeyPair(this, key, value);
                else
                {
                    dbKvp.Clear();
                    dbKvp.Delete();
                    NewKeyPair(this, key, value);
                }
            }
        }

        public dynamic SafeGet(string key)
        {
            return Db.SQL<DKeyValuePair>($"SELECT t FROM {KvpTable} t " +
                                         "WHERE t.Dictionary =? AND t.Key =? ", this, key).First?.Value;
        }

        public void OnDelete()
        {
            foreach (var pair in KeyValuePairs)
                pair.Delete();
        }

        public ICollection<string> Keys => KeyValuePairs.Select(i => i.Key).ToList();

        public ICollection<object> Values => KeyValuePairs.Select(i => i.Value).ToList();
    }
}