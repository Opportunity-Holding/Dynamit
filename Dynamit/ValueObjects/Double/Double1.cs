using System.Globalization;
using Starcounter;

namespace Dynamit.ValueObjects.Double
{
    [Database]
    public class Double1 : ValueObject
    {
        public double content { get; internal set; }

        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
    }
}