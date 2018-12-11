using Starcounter;
using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Ushort
{
    [Database]
    public class Ushort1 : ValueObject
    {
        public ushort content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 18;
    }
}