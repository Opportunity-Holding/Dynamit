using Starcounter;
using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Bool
{
    [Database]
    public class Bool1 : ValueObject
    {
        public bool content { get; internal set; }
        public override string ToString() => content.ToString();
        internal override long ByteCount => 20;
    }
}