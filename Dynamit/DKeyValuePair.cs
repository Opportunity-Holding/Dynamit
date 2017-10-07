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
        public DDictionary Dictionary;
        public readonly string Key;
        public int? ValueHash;
        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";
        private dynamic GetValueObject() => ValueObjectNo == null ? null : DbHelper.FromID(ValueObjectNo.Value);
        public ulong? ValueObjectNo;
        public void OnDelete() => ((object) GetValueObject())?.Delete();
        public static implicit operator KVP(DKeyValuePair pair) => new KVP(pair.Key, pair.Value);
        public long ByteCount => Encoding.UTF8.GetByteCount(Key) + (GetValueObject()?.ByteCount ?? 0) + 16;

        public dynamic Value
        {
            get => GetValueObject()?.content;
            private set
            {
                if (value == null) return;
                (ValueObjectNo, ValueHash) = ValueObject.Make((object) value);
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