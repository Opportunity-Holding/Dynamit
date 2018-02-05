using System;

namespace Dynamit
{
    /// <inheritdoc />
    public sealed class InvalidValueTypeException : Exception
    {
        /// <inheritdoc />
        public InvalidValueTypeException(Type type) : base("Illegal value type for dynamic table: " + type.FullName) { }

        /// <inheritdoc />
        public InvalidValueTypeException(Type type, string message)
            : base($"Illegal value type for dynamic table: {type.FullName}. {message}") { }
    }

    /// <inheritdoc />
    public sealed class MissingIDDictionaryException : Exception
    {
        /// <inheritdoc />
        public MissingIDDictionaryException(Type type)
            : base($"Missing IDDictionary interface implementation for type '{type.FullName}'") { }
    }

    /// <inheritdoc />
    public sealed class NestedDKeyValuePairDeclarationException : Exception
    {
        /// <inheritdoc />
        public NestedDKeyValuePairDeclarationException(Type type)
            : base($"Invalid DKeyValuePair subtype '{type.FullName}'. Cannot be nested inside another class") { }
    }

    /// <inheritdoc />
    public sealed class DListException : Exception
    {
        /// <inheritdoc />
        public DListException(Type type)
            : base($"Missing DListAttribute decoration for type '{type.FullName}'") { }
    }
}