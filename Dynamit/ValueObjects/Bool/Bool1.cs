using Starcounter;

namespace Dynamit.ValueObjects.Bool
{
    [Database]
    public class Bool1 : ValueObject
    {
        public bool content { get; set; }

        public override string ToString() => content.ToString();
    }    
}