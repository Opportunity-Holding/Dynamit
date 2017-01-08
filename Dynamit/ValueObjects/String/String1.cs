using Starcounter;

namespace Dynamit.ValueObjects.String
{
    [Database]
    public class String1 : ValueObject
    {
        public string content { get; set; }

        public override string ToString() => content;
    }
}