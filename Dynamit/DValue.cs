using Dynamit.ValueObjects;
using Dynamit.ValueObjects.String;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit
{
    [Database]
    public abstract class DValue : IEntity
    {
        public string ValueType => GetValueObject()?.content?.GetType().FullName ?? "<value is null>";
        public string ValueString => GetValueObject()?.ToString() ?? "null";
        public int? ValueHash;
        public ulong? ValueObjectNo;
        protected DValue(object value = null) => (ValueObjectNo, ValueHash) = ValueObject.Make(value);
        private dynamic GetValueObject() => ValueObjectNo == null ? null : DbHelper.FromID(ValueObjectNo.Value);
        public void OnDelete() => ((object) GetValueObject())?.Delete();

        public dynamic Value
        {
            get => GetValueObject()?.content;
            set
            {
                var valueObject = GetValueObject();
                if (value == null)
                {
                    if (valueObject != null)
                        Db.Delete(valueObject);
                    ValueObjectNo = null;
                    ValueHash = null;
                    return;
                }
                if (valueObject == null)
                {
                    (ValueObjectNo, ValueHash) = ValueObject.Make((object) value);
                    return;
                }
                if (valueObject.content == null)
                {
                    if (valueObject is String1 && value is string @string)
                    {
                        valueObject.content = value;
                        ValueHash = @string.GetHashCode();
                        return;
                    }
                }
                Db.Delete(valueObject);
                (ValueObjectNo, ValueHash) = ValueObject.Make((object) value);
            }
        }
    }
}