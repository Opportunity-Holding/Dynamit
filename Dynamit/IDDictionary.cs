namespace Dynamit
{
    /// <summary>
    /// A generic interface that gives Dynamit some meta information about the DDictionary table
    /// </summary>
    public interface IDDictionary<in TTable, out TKvp> where TTable : DDictionary, IDDictionary<TTable, TKvp>
        where TKvp : DKeyValuePair
    {
        /// <summary>
        /// The key-value pair constructor
        /// </summary>
        TKvp NewKeyPair(TTable dict, string key, object value = null);
    }
}