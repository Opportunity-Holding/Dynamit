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
}
