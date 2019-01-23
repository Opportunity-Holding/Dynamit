using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Long
{
    [Database]
    public class Long1 : ValueObject
    {
        public long content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 24;
    }
}