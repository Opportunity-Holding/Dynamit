using Dynamit.ValueObjects;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit
{
    [Database]
    public abstract class DElement : IEntity
    {
        public DList List;
        public int Index { get; internal set; }
        public int? ValueHash;
        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";
        private dynamic GetValueObject() => ValueObjectNo == null ? null : DbHelper.FromID(ValueObjectNo.Value);
        public ulong? ValueObjectNo;
        public void OnDelete() => ((object) GetValueObject())?.Delete();

        public dynamic Value
        {
            get => GetValueObject()?.content;
            private set
            {
                if (value == null) return;
                (ValueObjectNo, ValueHash) = ValueObject.Make((object) value);
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