using System;
using Dynamit.ValueObjects;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit
{
    [Database]
    public abstract class DElement : IEntity
    {
        public DList List { get; }
        public int Index { get; internal set; }
        public int? ValueHash { get; private set; }
        public TypeCode ValueTypeCode { get; private set; }
        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";
        private dynamic GetValueObject() => ValueObjectNo == null ? null : Db.FromId(ValueObjectNo.Value);
        public ulong? ValueObjectNo { get; private set; }
        public void OnDelete() => ((object) GetValueObject())?.Delete();

        public dynamic Value
        {
            get => GetValueObject()?.content;
            private set
            {
                if (value == null) return;
                (ValueObjectNo, ValueHash, ValueTypeCode) = ValueObject.Make((object) value);
            }
        }

        protected DElement(DList list, int index, object value = null)
        {
            List = list;
            Index = index;
            Value = value;
        }
    }
}