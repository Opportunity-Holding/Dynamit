using Starcounter;

namespace Dynamit.ValueObjects.Sbyte
{
    [Database]
    public class Sbyte1 : ValueObject
    {
        public sbyte content { get; set; }

        public override string ToString() => content.ToString();
    }
}