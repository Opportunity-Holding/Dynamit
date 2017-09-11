using System;

namespace Dynamit
{
    public sealed class InvalidValueTypeException : Exception
    {
        public InvalidValueTypeException(Type type) : base("Illegal value type for dynamic table: " + type.FullName)
        {
        }

        public InvalidValueTypeException(Type type, string message)
            : base($"Illegal value type for dynamic table: {type.FullName}. {message}")
        {
        }
    }

    public sealed class MissingIDDictionaryException : Exception
    {
        public MissingIDDictionaryException(Type type)
            : base($"Missing IDDictionary interface implementation for type '{type.FullName}'")
        {
        }
    }

    public sealed class NestedDKeyValuePairDeclarationException : Exception
    {
        public NestedDKeyValuePairDeclarationException(Type type)
            : base($"Invalid DKeyValuePair subtype '{type.FullName}'. Cannot be nested inside another class")
        {
        }
    }

    public sealed class DListException : Exception
    {
        public DListException(Type type)
            : base($"Missing DListAttribute decoration for type '{type.FullName}'")
        {
        }
    }
}