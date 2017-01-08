using Starcounter;

namespace Dynamit
{
    [Database]
    public class Entity
    {
        public ulong ObjectNumber { get; }

        public Entity()
        {
            ObjectNumber = Dynamit.ObjectNumber.Next;
        }
    }
}
