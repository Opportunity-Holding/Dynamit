using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                Db.TransactAsync(() => x.Delete());

            DynamicProduct product = null;
            Db.TransactAsync(() =>
            {
                product = new DynamicProduct
                {
                    ["Label"] = "Double Espresso",
                    ["Product_ID"] = 42,
                    ["Product_date"] = new DateTime(2017, 01, 05),
                    ["Price"] = 3.25,
                    ["Group"] = "A1"
                };
            });

            var s = product["Product_date"].AddDays(2).ToString("O");

            dynamic pr;

            Db.TransactAsync(() =>
            {
                pr = new DynamicProduct();
                pr.A = "My favourite";
                pr.Aswoo = 123321.1;
                pr.Goog = DateTime.Now;
            });

            dynamic dsa = product;
            var dx = dsa.Label;

            Db.TransactAsync(() =>
            {
                dsa.Banana = 123123.1;
            });

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