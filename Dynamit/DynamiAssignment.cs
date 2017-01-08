using Starcounter;

namespace Dynamit
{
    [Database]
    public class DynamiAssignment
    {
        public DynamiRow Row { get; }
        public string Key { get; }
        public ulong ContentNo { get; set; }

        public dynamic DynContent()
        {
            dynamic result = DbHelper.FromID(ContentNo);
            return result.content;
        }

        public DynamiAssignment(DynamiRow obj, string key, ulong contentNo)
        {
            Row = obj;
            Key = key;
            ContentNo = contentNo;
        }

        public void Remove()
        {
            Db.Transact(() =>
            {
                DbHelper.FromID(ContentNo).Delete();
                Key.Delete();
                this.Delete();
            });
        }

        public static DynamiAssignment Create<T>(DynamiRow obj, string key, T content) where T : class
        {
            return new DynamiAssignment(obj, key, content.GetObjectNo());
        }
    }
}