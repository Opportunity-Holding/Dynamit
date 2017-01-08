using Starcounter;

namespace Dynamit.ValueObjects.Short
{
    [Database]
    public class Short1 : ValueObject
    {
        public short content { get; set; }

        public override string ToString() => content.ToString();
    }
}