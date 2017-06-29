using System.Globalization;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Decimal
{
    [Database]
    public class Decimal1 : ValueObject
    {
        public decimal content { get; internal set; }
        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
    }
}