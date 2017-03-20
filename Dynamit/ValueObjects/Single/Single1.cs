﻿using System.Globalization;
using Starcounter;

namespace Dynamit.ValueObjects.Single
{
    [Database]
    public class Single1 : ValueObject
    {
        public float content { get; internal set; }

        public override string ToString() => content.ToString(CultureInfo.CurrentCulture);
    }
}