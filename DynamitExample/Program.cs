using Dynamit;
using Starcounter;

namespace DynamitExample
{
    public class Program
    {
        public static void Main()
        {
            DynamitConfig.Init();
            foreach (var x in DB.All<DDictionary>())
                Db.Transact(() => x.Delete());

            var dict1 = Db.Transact(() => new MyDict
            {
                ["Name"] = "Kalle",
                ["Age"] = 28,
                ["Group"] = "ABC",
                ["ID"] = 1
            });

            var dict2 = Db.Transact(() => new MyDict
            {
                ["Name"] = "Anders",
                ["Age"] = 32,
                ["Group"] = "ABC",
                ["ID"] = 2
            });
            var dict3 = Db.Transact(() => new MyDict
            {
                ["Name"] = "Lisa",
                ["Age"] = 28,
                ["Group"] = "BCD",
                ["ID"] = 3
            });
            var dict4 = Db.Transact(() => new MyDict
            {
                ["Name"] = "Lotta",
                ["Age"] = 31,
                ["Group"] = "ABC",
                ["ID"] = 4
            });


            var a = Finder.DDictionary<MyDict>(new TDictionary
            {
                ["Age"] = 28
            });

            var a1 = Finder.DDictionary<MyDict>(new TDictionary
            {
                ["Age"] = 28,
                ["Group"] = "BCD"
            });

            var all = Finder.DDictionary<MyDict>();

            var b = Finder.DDictionary<MyDict>(
                dict => dict.SafeGet("Age") > 28,
                dict => dict.ContainsKey("Group") && dict["Group"] == "BCD"
            );


            var c = "";
        }
    }

    public class MyPair : DKeyValuePair
    {
        public MyPair(DDictionary dict, string key, object value = null) : base(dict, key, value)
        {
        }
    }

    [DDictionary(typeof(MyPair))]
    public class MyDict : DDictionary
    {
        protected override DKeyValuePair NewKeyPair(DDictionary dict, string key, object value = null)
        {
            return new MyPair(dict, key, value);
        }
    }
}