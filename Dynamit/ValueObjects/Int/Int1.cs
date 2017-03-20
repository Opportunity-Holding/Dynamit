using Starcounter;

namespace Dynamit.ValueObjects.Int
{
    [Database]
    public class Int1 : ValueObject
    {
        public int content { get; internal set; }

        public override string ToString() => content.ToString();
    }
}