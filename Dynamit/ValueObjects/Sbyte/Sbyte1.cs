using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Sbyte
{
    [Database]
    public class Sbyte1 : ValueObject
    {
        public sbyte content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 17;
    }
}