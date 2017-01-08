using Starcounter;

namespace Dynamit.ValueObjects.Long
{
    [Database]
    public class Long1 : ValueObject
    {
        public long content { get; set; }

        public override string ToString() => content.ToString();
    }
}