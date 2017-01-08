using Starcounter;

namespace Dynamit.ValueObjects.Byte
{
    [Database]
    public class Byte1 : ValueObject
    {
        public byte content { get; set; }

        public override string ToString() => content.ToString();
    }
}