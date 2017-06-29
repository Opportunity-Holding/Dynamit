using Starcounter;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Uint
{
    [Database]
    public class Uint1 : ValueObject
    {
        public uint content { get; internal set; }
        public override string ToString() => content.ToString();
    }
}