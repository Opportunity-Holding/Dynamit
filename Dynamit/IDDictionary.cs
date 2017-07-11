namespace Dynamit
{
    public interface IDDictionary<in TTable, out TKvp> where TTable : DDictionary, IDDictionary<TTable, TKvp>
        where TKvp : DKeyValuePair
    {
        TKvp NewKeyPair(TTable dict, string key, object value = null);
    }
}