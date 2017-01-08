using Starcounter;

namespace Dynamit.ValueObjects.Uint
{
    [Database]
    public class Uint1 : ValueObject
    {
        public uint content { get; set; }

        public override string ToString() => content.ToString();
    }
}