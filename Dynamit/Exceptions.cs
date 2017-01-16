using System;

namespace Dynamit
{
    public class ScDictionaryException : Exception
    {
        public ScDictionaryException(Type type) : base("Missing 'ScDictionaryAttribute' " +
                                                       $"decoration for type '{type.FullName}'")
        {
        }
    }
}