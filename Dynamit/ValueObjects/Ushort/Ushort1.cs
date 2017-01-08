using Starcounter;

namespace Dynamit.ValueObjects.Ushort
{
    [Database]
    public class Ushort1 : ValueObject
    {
        public ushort content { get; set; }

        public override string ToString() => content.ToString();
    }
}