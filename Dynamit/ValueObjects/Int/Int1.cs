using Starcounter;

namespace Dynamit.ValueObjects.Int
{
    [Database]
    public class Int1 : ValueObject
    {
        public int content { get; set; }

        public override string ToString() => content.ToString();
    }
}