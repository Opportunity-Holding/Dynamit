﻿using System;

namespace Dynamit
{
    internal class DDictionaryException : Exception
    {
        public DDictionaryException(Type type)
            : base($"Missing DDictionaryAttribute decoration for type '{type.FullName}'")
        {
        }
    }

    internal class DListException : Exception
    {
        public DListException(Type type)
            : base($"Missing DListAttribute decoration for type '{type.FullName}'")
        {
        }
    }
}