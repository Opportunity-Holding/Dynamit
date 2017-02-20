using System;
using System.Collections.Generic;
using System.Dynamic;
using Dynamit.ValueObjects.Bool;
using Dynamit.ValueObjects.Byte;
using Dynamit.ValueObjects.DateTime;
using Dynamit.ValueObjects.Decimal;
using Dynamit.ValueObjects.Double;
using Dynamit.ValueObjects.Int;
using Dynamit.ValueObjects.Long;
using Dynamit.ValueObjects.Sbyte;
using Dynamit.ValueObjects.Short;
using Dynamit.ValueObjects.Single;
using Dynamit.ValueObjects.String;
using Dynamit.ValueObjects.Uint;
using Dynamit.ValueObjects.Ulong;
using Dynamit.ValueObjects.Ushort;
using Starcounter;

namespace Dynamit
{
    [Database]
    public abstract class DKeyValuePair
    {
        public DDictionary Dictionary;
        public readonly string Key;
        public int? ValueHash;

        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";

        private dynamic GetValueObject() => ValueObjectNo == null ? null : DbHelper.FromID(ValueObjectNo.Value);

        public dynamic Value
        {
            get { return GetValueObject()?.content; }
            private set
            {
                if (value == null) return;
                int? hash;
                ValueObjectNo = MakeValueObject(value, out hash);
                ValueHash = hash;
            }
        }

        public ulong? ValueObjectNo;

        protected DKeyValuePair(DDictionary dict, string key, object value = null)
        {
            Dictionary = dict;
            Key = key;
            Value = value;
        }

        private static ulong? MakeValueObject(dynamic value, out int? hash)
        {
            if (value == null)
            {
                hash = null;
                return null;
            }
            if (value is IDynamicMetaObjectProvider)
            {
                ValueTypes valueType;
                var obj = Helper.GetStaticType(value, out valueType);
                hash = obj.GetHashCode();
                switch (valueType)
                {
                    case ValueTypes.String:
                        return new String1 {content = obj}.GetObjectNo();
                    case ValueTypes.Bool:
                        return new Bool1 {content = obj}.GetObjectNo();
                    case ValueTypes.Int:
                        return new Int1 {content = obj}.GetObjectNo();
                    case ValueTypes.Long:
                        return new Long1 {content = obj}.GetObjectNo();
                    case ValueTypes.Decimal:
                        return new Decimal1 {content = obj}.GetObjectNo();
                    case ValueTypes.DateTime:
                        return new DateTime1 {content = obj}.GetObjectNo();
                }
            }
            hash = value.GetHashCode();
            if (value is string) return new String1 {content = value}.GetObjectNo();
            if (value is bool) return new Bool1 {content = value}.GetObjectNo();
            if (value is byte) return new Byte1 {content = value}.GetObjectNo();
            if (value is DateTime) return new DateTime1 {content = value}.GetObjectNo();
            if (value is decimal) return new Decimal1 {content = value}.GetObjectNo();
            if (value is double) return new Double1 {content = value}.GetObjectNo();
            if (value is int) return new Int1 {content = value}.GetObjectNo();
            if (value is long) return new Long1 {content = value}.GetObjectNo();
            if (value is sbyte) return new Sbyte1 {content = value}.GetObjectNo();
            if (value is short) return new Short1 {content = value}.GetObjectNo();
            if (value is float) return new Single1 {content = value}.GetObjectNo();
            if (value is uint) return new Uint1 {content = value}.GetObjectNo();
            if (value is ulong) return new Ulong1 {content = value}.GetObjectNo();
            if (value is ushort) return new Ushort1 {content = value}.GetObjectNo();
            hash = null;
            return null;
        }

        internal void Clear()
        {
            var valueObject = GetValueObject();
            if (valueObject != null)
            {
                Db.Delete(valueObject);
            }
        }

        public static implicit operator KeyValuePair<string, object>(DKeyValuePair pair)
        {
            return new KeyValuePair<string, object>(pair.Key, pair.Value);
        }
    }
}