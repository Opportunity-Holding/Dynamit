using Starcounter;

namespace Dynamit.ValueObjects.Sbyte
{
    [Database]
    public class Sbyte1 : ValueObject
    {
        public sbyte content { get; internal set; }

        public override string ToString() => content.ToString();
    }
}