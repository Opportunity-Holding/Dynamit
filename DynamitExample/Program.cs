using System;
using System.Collections.Generic;
using System.Linq;
using Dynamit;
using Starcounter;

namespace DynamitExample
{
    public class Program
    {
        public static void Main()
        {
            DynamitConfig.Init();
            foreach (var x in Db.SQL<DDictionary>($"SELECT t FROM {typeof(DDictionary).FullName} t"))
                Db.Transact(() => x.Delete());

            var product = Db.Transact(() => new DynamicProduct
            {
                ["Label"] = "Double Espresso",
                ["Product_ID"] = 42,
                ["Product_date"] = new DateTime(2017, 01, 05),
                ["Price"] = 3.25,
                ["Group"] = "A1"
            });

            var s = product["Product_date"].AddDays(2).ToString("O");

            var o = product is IDictionary<string, dynamic>;

            var d = Finder.Select<DynamicProduct>(row => row["Product_ID"] > 10).First()["Label"];

            var sdsa =
                Finder.Select<DynamicProduct>(new Dictionary<string, dynamic> {["Group"] = "A1"})
                    .Where(da => da.SafeGet("Price") > 3);

            var xs = "";
        }
    }

    [DDictionary(typeof(DynamicProductKeyValuePair))]
    public class DynamicProduct : DDictionary
    {
        protected override DKeyValuePair NewKeyPair(DDictionary dict, string key, object value = null)
        {
            return new DynamicProductKeyValuePair(dict, key, value);
        }
    }

    public class DynamicProductKeyValuePair : DKeyValuePair
    {
        public DynamicProductKeyValuePair(DDictionary dict, string key, object value = null) : base(dict, key, value)
        {
        }
    }
}