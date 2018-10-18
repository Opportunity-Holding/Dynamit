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
    public abstract class DDictionary : IDictionary<string, object>, ICollection<KVP>, IReadOnlyDictionary<string, object>, IReadOnlyCollection<KVP>,
        IEnumerable<KVP>, IEnumerable, IEntity, IDynamicMetaObjectProvider
    {
        /// <summary>
        /// The name of the table where key-value pairs are stored
        /// </summary>
        public string KvpTable { get; }

        protected virtual object GetDeclaredMemberValue(string key) => null;
        protected virtual void SetDeclaredMemberValue(string key, object value) { }

        /// <inheritdoc />
        public void Add(KVP item)
        {
            switch (item.Key?.FirstOrDefault())
            {
                case null: throw new ArgumentNullException(nameof(item.Key));
                case '\0': throw new ArgumentException("Key cannot be the empty string");
                case '$': throw new ArgumentException("Keys of dictionary members cannot begin with '$'");
                case var _ when item.Value == null: return;
                case var _ when ContainsKey(item.Key): throw new ArgumentException($"Error: key '{item.Key}' already in dictionary");
                default:
                    MakeKeyPair(item.Key, item.Value);
                    break;
            }
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

        private DKeyValuePair GetPair(string key) => Db.SQL<DKeyValuePair>(KSQL, this, key).FirstOrDefault();

        /// <inheritdoc />
        public bool Remove(string key)
        {
            switch (key.FirstOrDefault())
            {
                case '\0': return false;
                case '$': throw new ArgumentException("Cannot remove declared members");
                case var _ when GetPair(key) is DKeyValuePair pair:
                    pair.Delete();
                    return true;
                default: return false;
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            switch (key.FirstOrDefault())
            {
                case '\0':
                    value = null;
                    return false;
                case '$':
                    value = GetDeclaredMemberValue(key.Substring(1));
                    return value != null;
                case var _ when GetPair(key) is DKeyValuePair pair:
                    value = pair.Value;
                    return value != null;
                default:
                    value = null;
                    return false;
            }
        }

        public bool TryGetValue(string key, out object value, out string actualKey)
        {
            actualKey = null;
            switch (key.FirstOrDefault())
            {
                case '\0':
                    value = null;
                    return false;
                case '$':
                    value = GetDeclaredMemberValue(key.Substring(1));
                    return value != null;
                case var _ when GetPair(key) is DKeyValuePair pair:
                    value = pair.Value;
                    actualKey = pair.Key;
                    return value != null;
                default:
                    value = null;
                    return false;
            }
        }

        public dynamic this[string key]
        {
            get
            {
                switch (key.FirstOrDefault())
                {
                    case '\0': return null;
                    case '$': return GetDeclaredMemberValue(key.Substring(1));
                    case var _ when GetPair(key) is DKeyValuePair pair: return pair.Value;
                    default: throw new KeyNotFoundException();
                }
            }
            set
            {
                switch (key.FirstOrDefault())
                {
                    case '\0': throw new ArgumentException("Key cannot be the empty string");
                    case '$':
                        SetDeclaredMemberValue(key.Substring(1), value);
                        break;
                    case var _ when GetPair(key) is DKeyValuePair pair:
                        pair.Delete();
                        if (value == null) return;
                        if (value is IDynamicMetaObjectProvider)
                            value = ValueObject.GetStaticType(value);
                        MakeKeyPair(key, value);
                        break;
                    default:
                        if (value == null) return;
                        if (value is IDynamicMetaObjectProvider)
                            value = ValueObject.GetStaticType(value);
                        MakeKeyPair(key, value);
                        break;
                }
            }
        }

        public IEnumerator<KVP> GetEnumerator() => _kvPairs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Contains(KVP item) => _kvPairs.Contains(item);

        bool ICollection<KVP>.Remove(KVP item)
        {
            var pair = GetPair(item.Key);
            if (pair == null) return false;
            if (item.Value?.Equals((object) pair.Value) == true)
                return Remove(item.Key);
            return false;
        }

        public int Count => KeyValuePairs.Count();
        public bool IsReadOnly => false;
        public bool ContainsKey(string key) => TryGetValue(key, out _);

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
        private object Get(string key) => SafeGet(key);
        private object Set(string key, object value) => this[key] = value;

        public void Clear()
        {
            foreach (var pair in KeyValuePairs) pair.Delete();
        }

        /// <summary>
        /// Gets the value of a key from a DDictionary, or null if the dictionary does not contain the key.
        /// </summary>
        public dynamic SafeGet(string key)
        {
            TryGetValue(key, out var value);
            return value;
        }

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