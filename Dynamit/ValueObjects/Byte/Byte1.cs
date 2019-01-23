using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Byte
{
    [Database]
    public class Byte1 : ValueObject
    {
        public byte content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 17;
    }
}