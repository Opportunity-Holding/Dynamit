using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Dynamit;
using Starcounter.Nova;
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
                product = Db.Insert<Product>();
                product["Label"] = "Double Espresso";
                product["Product_ID"] = 42;
                product["Product_date"] = new DateTime(2017, 01, 05);
                product["Price"] = 3.25;
                product["Group"] = "A1";
            });

            var s = product["Product_date"].AddDays(2).ToString("O");

            var sdsa = Finder<Product>.All;

            dynamic pr = null;

            Db.TransactAsync(() =>
            {
                pr = Db.Insert<Product>();
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
                prod1 = Db.Insert<DynamicProduct>();
                prod1.MyDynamicMember = "hej";
            });
            var s1 = prod1.MyDynamicMember;

            var le = s1.Length;

            Finder<DynamicProduct>.Where("MyDynamicMember", EQUALS, "hej");
        }
    }

    [Database]
    public abstract class MyDbClass
    {
        public virtual string Mstr { get; set; }
        public virtual Other Myother { get; set; }
        public virtual Third Mythird => new Third();
        public virtual DynamicProduct Product { get; set; }
    }

    [Database]
    public class Other { }

    public class Third { }

    [Database]
    public abstract class DynamicProduct : DDictionary, IDDictionary<DynamicProduct, DynamicProductKVP>
    {
        public DynamicProductKVP NewKeyPair(DynamicProduct dict, string key, object value = null)
        {
            return DynamicProductKVP.Create(dict, key, value);
        }
    }

    [Database]
    public abstract class DynamicProductKVP : DKeyValuePair
    {
        public static DynamicProductKVP Create(DynamicProduct dict, string key, object value)
        {
            return Create<DynamicProductKVP>(dict, key, value);
        }
    }

    [Database]
    public abstract class Product : DDictionary, IDDictionary<Product, ProductKVP>
    {
        public ProductKVP NewKeyPair(Product dict, string key, object value = null)
        {
            return ProductKVP.Create(dict, key, value);
        }
    }

    [Database]
    public abstract class ProductKVP : DKeyValuePair
    {
        public static ProductKVP Create(Product dict, string key, object value)
        {
            return Create<ProductKVP>(dict, key, value);
        }
    }

    [Database]
    public abstract class Condition
    {
        public virtual string PropertyName { get; set; }

        //public OperatorsEnum Operator;
        public virtual ConditionValue Value { get; set; } // This is our DValue member

        public void OnDelete() => Value?.Delete();
    }

    [Database]
    public abstract class ConditionValue : DValue
    {
        public ConditionValue(object value) => Value = value;
    }
}