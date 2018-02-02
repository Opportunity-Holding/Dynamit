using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Dynamit.ValueObjects;
using Starcounter;
using static System.Dynamic.BindingRestrictions;
using static System.Linq.Expressions.Expression;
using static System.Reflection.BindingFlags;
using KVP = System.Collections.Generic.KeyValuePair<string, object>;

#pragma warning disable 1591

namespace Dynamit
{
    /// <inheritdoc cref="IDictionary{TKey,TValue}" />
    /// <summary>
    /// A dynamic persistent database type for Starcounter applications
    /// </summary>
    [Database]
    public abstract class DDictionary : IDictionary<string, object>, ICollection<KVP>,
        IReadOnlyDictionary<string, object>, IReadOnlyCollection<KVP>, IEnumerable<KVP>, IEnumerable, IEntity,
        IDynamicMetaObjectProvider
    {
        /// <summary>
        /// The name of the table where key-value pairs are stored
        /// </summary>
        public string KvpTable { get; }

        /// <inheritdoc />
        public void Add(KVP item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            if (item.Value == null) return;
            if (ContainsKey(item.Key))
                throw new ArgumentException($"Error: key '{item.Key}' already in dictionary");
            MakeKeyPair(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void CopyTo(KVP[] array, int arrayIndex)
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

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            try
            {
                var obj = Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault();
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
            if (key == null) throw new ArgumentNullException(nameof(key));
            var match = Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault();
            value = match?.Value;
            return match != null;
        }

        public dynamic this[string key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                var match = Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault();
                if (match == null) throw new KeyNotFoundException();
                return match.Value;
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (value is IDynamicMetaObjectProvider)
                    value = ValueObject.GetStaticType(value);
                Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault()?.Delete();
                if (value == null) return;
                MakeKeyPair(key, value);
            }
        }

        public IEnumerator<KVP> GetEnumerator() => _kvPairs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Contains(KVP item) => _kvPairs.Contains(item);
        public bool Remove(KVP item) => Remove(item.Key);
        public int Count => KeyValuePairs.Count();
        public bool IsReadOnly => false;
        public bool ContainsKey(string key) => Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault() != null;
        public void Add(string key, object value) => Add(new KVP(key, value));
        public void OnDelete() => Clear();
        public ICollection<string> Keys => KeyValuePairs.Select(i => i.Key).ToList();
        public ICollection<object> Values => KeyValuePairs.Select(i => i.Value).ToList();
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression e) => new DMetaObject(e, this);
        protected DDictionary() => KvpTable = DynamitConfig.KvpMappings[GetType().FullName];

        private void MakeKeyPair(string k, dynamic v)
        {
            if (k.ElementAtOrDefault(0) == '$') return;
            ((dynamic) this).NewKeyPair((dynamic) this, k, v);
        }

        private string KSQL => $"SELECT t FROM {KvpTable} t WHERE t.Dictionary =? AND t.Key =?";
        private string TSQL => $"SELECT t FROM {KvpTable} t WHERE t.Dictionary =?";
        public IEnumerable<DKeyValuePair> KeyValuePairs => Db.SQL<DKeyValuePair>(TSQL, this);
        private IEnumerable<KVP> _kvPairs => Db.SQL<DKeyValuePair>(TSQL, this).Select(p => new KVP(p.Key, p.Value));
        private object Get(string key) => ContainsKey(key) ? this[key] : null;
        private object Set(string key, object value) => this[key] = value;

        public void Clear()
        {
            foreach (var pair in KeyValuePairs) pair.Delete();
        }

        /// <summary>
        /// Gets the value of a key from a DDictionary, or null if the dictionary does not contain the key.
        /// </summary>
        public dynamic SafeGet(string key) => Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault()?.Value;

        private class DMetaObject : DynamicMetaObject
        {
            internal DMetaObject(Expression e, DDictionary d) : base(e, BindingRestrictions.Empty, d) { }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) => new DynamicMetaObject
            (
                expression: Call
                (
                    instance: Convert(Expression, LimitType),
                    method: typeof(DDictionary).GetMethod(nameof(Get), Instance | NonPublic)
                            ?? throw new Exception("Error when binding get method"),
                    arguments: Constant(binder.Name)
                ),
                restrictions: GetTypeRestriction(Expression, LimitType)
            );

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) =>
                new DynamicMetaObject
                (
                    expression: Call
                    (
                        instance: Convert(Expression, LimitType),
                        method: typeof(DDictionary).GetMethod(nameof(Set), Instance | NonPublic)
                                ?? throw new Exception("Error when binding set method"),
                        arg0: Constant(binder.Name),
                        arg1: Convert(value.Expression, typeof(object))
                    ),
                    restrictions: GetTypeRestriction(Expression, LimitType)
                );

            public override IEnumerable<string> GetDynamicMemberNames() => ((DDictionary) Value).Keys;
        }
    }
}