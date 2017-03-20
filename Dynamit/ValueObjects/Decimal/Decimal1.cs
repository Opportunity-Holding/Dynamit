﻿using System.Globalization;
using Starcounter;

namespace Dynamit.ValueObjects.Decimal
{
    [Database]
    public class Decimal1 : ValueObject
    {
        public decimal content { get; internal set; }

        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
    }
}