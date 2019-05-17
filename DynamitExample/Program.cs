using System;
using System.Collections.Generic;
using System.Linq;
using Dynamit;
using Starcounter;
using static Dynamit.Operator;

// ReSharper disable All

namespace DynamitExample
{
    public class Program
    {
        public static void Main()
        {
            #region

            DynamitConfig.Init();
            foreach (var x in Db.SQL<DDictionary>($"SELECT t FROM {typeof(DDictionary).FullName} t"))
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

            var prod = Finder<Product>.First(("Product_ID", EQUALS, 42), ("Price", EQUALS, 3.25));

            var c = prod["Product_ID"];

            Finder<Product>
                .Where(("Group", EQUALS, "A1")) // C#7 ValueTuple literal
                .Where(da => da.SafeGet("Price") > 3); // regular LINQ

            var xs = "";

            #endregion

            dynamic prod1 = null;

            Db.TransactAsync(() =>
            {
                prod1 = new DynamicProduct();
                prod1.MyDynamicMember = "hej";
            });
            var s1 = prod1.MyDynamicMember;

            var le = s1.Length;

            Finder<DynamicProduct>.Where("MyDynamicMember", EQUALS, "hej");
        }
    }

    [Database]
    public class MyDbClass
    {
        public string Mstr { get; set; }
        public Other Myother { get; set; }
        public Third Mythird => new Third();
        public DynamicProduct Product { get; set; }
    }

    [Database]
    public class Other { }

    public class Third { }

    [Database]
    public class DynamicProduct : DDictionary, IDDictionary<DynamicProduct, DynamicProductKVP>
    {
        public DynamicProductKVP NewKeyPair(DynamicProduct dict, string key, object value = null)
        {
            return new DynamicProductKVP(dict, key, value);
        }
    }

    [Database]
    public class DynamicProductKVP : DKeyValuePair
    {
        public DynamicProductKVP(DDictionary dict, string key, object value = null) : base(dict, key, value) { }
    }

    [Database]
    public class Product : DDictionary, IDDictionary<Product, ProductKVP>
    {
        public ProductKVP NewKeyPair(Product dict, string key, object value = null)
        {
            return new ProductKVP(dict, key, value);
        }
    }

    [Database]
    public class ProductKVP : DKeyValuePair
    {
        public ProductKVP(DDictionary dict, string key, object value = null) : base(dict, key, value) { }
    }

    [Database]
    public class Condition : IEntity
    {
        public string PropertyName { get; set; }

        //public OperatorsEnum Operator;
        public ConditionValue Value { get; set; } // This is our DValue member

        public void OnDelete() => Value?.Delete();
    }

    [Database]
    public class ConditionValue : DValue
    {
        public ConditionValue(object value) => Value = value;
    }
}