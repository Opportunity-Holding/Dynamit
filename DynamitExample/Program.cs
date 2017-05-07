using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamit;
using Starcounter;
using static Dynamit.Operators;

namespace DynamitExample
{
    public class Program
    {
        public static void Main()
        {
            DynamitConfig.Init();
            foreach (var x in Db.SQL<DDictionary>($"SELECT t FROM {typeof(DDictionary).FullName} t"))
                Db.TransactAsync(() => x.Delete());
            foreach (var x in Db.SQL<DList>($"SELECT t FROM {typeof(DList).FullName} t"))
                Db.TransactAsync(() => x.Delete());

            #region DDictionary tests

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

            dynamic pr = null;

            Db.TransactAsync(() =>
            {
                pr = new DynamicProduct();
                pr.A = "My favourite";
                pr.Aswoo = 123321.1;
                pr.Goog = DateTime.Now;
            });

            dynamic dsa = product;
            var dx = dsa.Label;

            var g = dsa.Product_date.AddDays(1).ToString();

            Db.TransactAsync(() => { dsa.Banana = 123123.1; });

            var o = product is IDictionary<string, dynamic>;

            var sdsa = Finder<DynamicProduct>
                 .Where(new Conditions {["Group", EQUALS] = "A1"})
                .Where(da => da.SafeGet("Price") > 3);

            #endregion


            DynamicList list;
            Db.TransactAsync(() =>
            {
                list = new DynamicList
                {
                    "Showo",
                    123321.1,
                    DateTime.Now
                };
            });

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

    [DList(typeof(DynamicListElement))]
    public class DynamicList : DList
    {
        protected override DElement NewElement(DList dict, int index, object value = null)
        {
            return new DynamicListElement(dict, index, value);
        }
    }

    public class DynamicListElement : DElement
    {
        public DynamicListElement(DList list, int index, object value = null) : base(list, index, value)
        {
        }
    }
}