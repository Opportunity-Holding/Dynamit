using System;
using System.Collections.Generic;
using System.Text;
using Dynamit.ValueObjects;
using Starcounter;
using Starcounter.Nova;
using KVP = System.Collections.Generic.KeyValuePair<string, object>;

namespace Dynamit
{
    /// <inheritdoc />
    /// <summary>
    /// The abstract base class for DDictionary key-value pairs
    /// </summary>
    [Database]
    public abstract class DKeyValuePair : IEntity
    {
        /// <summary>
        /// The dictionary that this key-value pair belongs to
        /// </summary>
        public DDictionary Dictionary { get; }

        /// <summary>
        /// The key of the key-value pair
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The hashcode of the value of the key-value pair
        /// </summary>
        public int? ValueHash { get; private set; }

        /// <summary>
        /// The typecode of the value of the key-value pair
        /// </summary>
        public TypeCode ValueTypeCode { get; internal set; }

        /// <summary>
        /// The object number of the value object of the key-value pair
        /// </summary>
        public ulong? ValueObjectNo { get; private set; }

        /// <summary>
        /// The name of the CLR type of the value of the key-value pair
        /// </summary>
        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";

        /// <summary>
        /// A string representation of the value of the key-value pair
        /// </summary>
        public string ValueString => GetValueObject()?.ToString() ?? "null";

        private dynamic GetValueObject() => ValueObjectNo == null ? null : Db.FromId(ValueObjectNo.Value);

        /// <inheritdoc />
        public void OnDelete() => ((object) GetValueObject())?.Delete();

        /// <summary>
        /// An operator for implicit conversion from a DKeyValuePair instance to a <see cref="KeyValuePair{TKey,TValue}"/>
        /// </summary>
        public static implicit operator KVP(DKeyValuePair pair) => new KVP(pair.Key, pair.Value);

        /// <summary>
        /// The number of bytes occupied by this key value-pair (estimated)
        /// </summary>
        public long ByteCount => Encoding.UTF8.GetByteCount(Key) + (GetValueObject()?.ByteCount ?? 0) + 16;

        /// <summary>
        /// A dynamic property for accessing the value of the key-value pair
        /// </summary>
        public dynamic Value
        {
            get => GetValueObject()?.content;
            private set
            {
                if (value == null) return;
                (ValueObjectNo, ValueHash, ValueTypeCode) = ValueObject.Make((object) value);
            }
        }

        /// <inheritdoc />
        protected DKeyValuePair(DDictionary dict, string key, object value = null)
        {
            Dictionary = dict;
            Key = key;
            Value = value;
        }
    }
}