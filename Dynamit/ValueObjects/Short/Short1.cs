using Starcounter;
using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Short
{
    [Database]
    public class Short1 : ValueObject
    {
        public short content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 18;
    }
}