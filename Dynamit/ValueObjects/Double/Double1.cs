using System.Globalization;
using Starcounter.Nova;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Double
{
    [Database]
    public class Double1 : ValueObject
    {
        public double content { get; internal set; }
        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
        internal override long ByteCount => 24;
    }
}