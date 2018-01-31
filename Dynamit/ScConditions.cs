namespace Dynamit
{
    internal struct ScConditions
    {
        internal ulong? ObjectNo { get; }
        internal string WhereString { get; }
        internal object[] Values { get; }

        public ScConditions(ulong? objectNo, string whereString, object[] values)
        {
            ObjectNo = objectNo;
            WhereString = whereString;
            Values = values;
        }
    }
}