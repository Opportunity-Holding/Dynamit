using Starcounter;

namespace Dynamit
{
    [Database]
    public class ObjectNumber
    {
        public ulong _number { get; private set; }
        public static ulong Next => Db.Transact(() => Get._number += 1);
        private static ObjectNumber Get => DB.First<ObjectNumber>() ?? Db.Transact(() => new ObjectNumber());

        private ObjectNumber()
        {
            _number = 1;
        }
    }
}