using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Int
{
    [Database]
    public class Int1 : ValueObject
    {
        public int content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 20;
    }
}