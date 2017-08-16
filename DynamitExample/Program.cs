using System;
using System.Collections.Generic;
using System.Linq;
using Dynamit;
using Starcounter;
using static Dynamit.Operators;
// ReSharper disable All

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

            Product product = null;
            Db.TransactAsync(() =>
            {
                product = new Product
                {
                    ["Label"] = "Double Espresso",
                    ["Product_ID"] = 42,
                    ["Product_date"] = new DateTime(2017, 01, 05),
                    ["Price"] = 3.25,
                    ["Group"] = "A1"
                };
            });

            var s = product["Product_date"].AddDays(2).ToString("O");

            var sdsa = Finder<Product>.All;

            dynamic pr = null;

            Db.TransactAsync(() =>
            {
                pr = new Product();
                pr.A = "My favourite";
                pr.Aswoo = 123321.1;
                pr.Goog = DateTime.Now;
            });

            dynamic dsa = product;
            var dx = dsa.Label;

            var g = dsa.Product_date.AddDays(1).ToString();

            var xas = Finder<Product>.All.Where(ob => ob["Product_ID"] == 42);

            Db.TransactAsync(() => dsa.Banana = 123123.1);

            var o = product is IDictionary<string, dynamic>;

            //var sdsa = Finder<DynamicProduct>
            //    .Where(new Conditions {["Group", EQUALS] = "A1"})
            //    .Where(da => da.SafeGet("Price") > 3);

            var prod = Finder<Product>.First(("Product_ID", "=", 42), ("Price", "=", 3.25));

            var c = prod["Product_ID"];

            Finder<Product>
                .Where(("Group", EQUALS, "A1"))	// C#7 ValueTuple literal
                .Where(da => da.SafeGet("Price") > 3);   // regular LINQ

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

    public class Product : DDictionary, IDDictionary<Product, ProductKVP>
    {
        public ProductKVP NewKeyPair(Product dict, string key, object value = null)
        {
            return new ProductKVP(dict, key, value);
        }
    }

    public class ProductKVP : DKeyValuePair
    {
        public ProductKVP(DDictionary dict, string key, object value = null) : base(dict, key, value)
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


    [Database]
    public class Condition : IEntity
    {
        public string PropertyName;
        //public OperatorsEnum Operator;
        public ConditionValue Value;  // This is our DValue member
        public void OnDelete() => Value?.Delete();
    }

    public class ConditionValue : DValue
    {
        public ConditionValue(object value) => Value = value;
    }


}