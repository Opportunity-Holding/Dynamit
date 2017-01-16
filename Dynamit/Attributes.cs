using System;

namespace Dynamit
{
    public class DDictionaryAttribute : Attribute
    {
        public Type KeyValuePairTable;

        public DDictionaryAttribute(Type keyValuePairTable)
        {
            KeyValuePairTable = keyValuePairTable;
        }
    }
}
