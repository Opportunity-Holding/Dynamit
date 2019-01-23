using Starcounter.Nova;

namespace Dynamit
{
    public interface IDynamitEntity
    {
        void OnDelete();
    }

    public static class DynamitEntityExtensions
    {
        public static void Delete(this IDynamitEntity entity)
        {
            entity.OnDelete();
            Db.Delete(entity);
        }
    }
}