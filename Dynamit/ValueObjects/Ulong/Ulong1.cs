using Starcounter;

namespace Dynamit.ValueObjects.Ulong
{
    [Database]
    public class Ulong1 : ValueObject
    {
        public ulong content { get; set; }

        public override string ToString() => content.ToString();
    }
}