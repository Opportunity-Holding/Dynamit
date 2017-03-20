using Starcounter;

namespace Dynamit.ValueObjects.DateTime
{
    [Database]
    public class DateTime1 : ValueObject
    {
        public System.DateTime content { get; internal set; }

        public override string ToString() => content.ToString("O");
    }
}