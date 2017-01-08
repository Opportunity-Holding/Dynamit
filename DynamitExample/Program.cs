using System;
using Dynamit;
using Dynamit.ValueObjects;
using Starcounter;

namespace DynamitExample
{
    public class Program
    {
        public static void Main()
        {
            foreach (var x in DB.All<ValueObject>())
                Db.Transact(() => x.Delete());

            DynamitConfig.Init();

            foreach (var x in DB.All<ScKeyValuePair>())
                Db.Transact(() => x.Delete());

            foreach (var x in DB.All<ScDictionary>())
                Db.Transact(() => x.Delete());

            var dict = Db.Transact(() => new MyDict
            {
                ["Bananas"] = 132.321,
                ["Swoopi"] = DateTime.Now,
                ["Grou"] = "Hello"
            });

            var c = "";

        }
    }

    public class MyPair : ScKeyValuePair
    {
        public MyPair(ScDictionary dict, string key, object value = null) : base(dict, key, value)
        {
        }
    }

    public class MyDict : ScDictionary
    {
        public MyDict() : base(typeof(MyPair))
        {
        }

        protected override ScKeyValuePair NewKeyPair(ScDictionary dict, string key, object value = null)
        {
            return new MyPair(dict, key, value);
        }
    }


}