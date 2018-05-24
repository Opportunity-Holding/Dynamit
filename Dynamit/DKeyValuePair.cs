using System;
using System.Text;
using Dynamit.ValueObjects;
using Starcounter;
using KVP = System.Collections.Generic.KeyValuePair<string, object>;

#pragma warning disable 1591

namespace Dynamit
{
    [Database]
    public abstract class DKeyValuePair : IEntity
    {
        public DDictionary Dictionary { get; }
        public string Key { get; }
        public int? ValueHash { get; private set; }
        public TypeCode ValueTypeCode { get; internal set; }
        public ulong? ValueObjectNo { get; private set; }

        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";
        private dynamic GetValueObject() => ValueObjectNo == null ? null : Db.FromId(ValueObjectNo.Value);
        public void OnDelete() => ((object) GetValueObject())?.Delete();
        public static implicit operator KVP(DKeyValuePair pair) => new KVP(pair.Key, pair.Value);
        public long ByteCount => Encoding.UTF8.GetByteCount(Key) + (GetValueObject()?.ByteCount ?? 0) + 16;

        public dynamic Value
        {
            get => GetValueObject()?.content;
            private set
            {
                if (value == null) return;
                (ValueObjectNo, ValueHash, ValueTypeCode) = ValueObject.Make((object) value);
            }
        }

        protected DKeyValuePair(DDictionary dict, string key, object value = null)
        {
            try
            {
                Dictionary = dict;
                Key = key;
                Value = value;
            }
            catch (InvalidValueTypeException e)
            {
                this.CancelConstructor(e);
            }
        }
    }
}