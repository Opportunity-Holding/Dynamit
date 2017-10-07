using System.Globalization;
using Starcounter;

#pragma warning disable 1591

namespace Dynamit.ValueObjects.Single
{
    [Database]
    public class Single1 : ValueObject
    {
        public float content { get; internal set; }
        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
        internal override long ByteCount => 20;
    }
}