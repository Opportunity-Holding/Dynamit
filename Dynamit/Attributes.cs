using System;

namespace Dynamit
{
    /// <summary>
    /// Used to decorate DList sub classes and provide the
    /// type for their elements.
    /// </summary>
    public class DListAttribute : Attribute
    {
        internal readonly Type ElementTable;

        /// <summary>
        /// Creates a new instance of the DListAttribute class
        /// </summary>
        /// <param name="elementTable">The type for the table where elements are to be stored</param>
        public DListAttribute(Type elementTable) => ElementTable = elementTable;
    }
}