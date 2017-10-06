using System;

namespace Dynamit
{
    /// <inheritdoc />
    /// <summary>
    /// Used to decorate DList sub classes and provide the
    /// type for their elements.
    /// </summary>
    public class DListAttribute : Attribute
    {
        internal readonly Type ElementTable;

        /// <inheritdoc />
        /// <param name="elementTable">The type for the table where elements are to be stored</param>
        public DListAttribute(Type elementTable) => ElementTable = elementTable;
    }
}