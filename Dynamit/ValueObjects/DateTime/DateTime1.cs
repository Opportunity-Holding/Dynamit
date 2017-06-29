using Starcounter;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.DateTime
{
    [Database]
    public class DateTime1 : ValueObject
    {
        public System.DateTime content { get; internal set; }
        public override string ToString() => content.ToString("O");
    }
}