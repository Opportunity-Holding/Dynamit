using System;

namespace Dynamit
{
    public class DDictionaryAttribute : Attribute
    {
        public readonly Type KeyValuePairTable;

        public DDictionaryAttribute(Type keyValuePairTable)
        {
            KeyValuePairTable = keyValuePairTable;
        }
    }

    public class DListAttribute : Attribute
    {
        public readonly Type ElementTable;

        public DListAttribute(Type elementTable)
        {
            ElementTable = elementTable;
        }
    }
}
