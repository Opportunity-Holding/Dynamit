using Dynamit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dynamit.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Starcounter;
using static System.Diagnostics.Debug;
using static Dynamit.Operator;

// ReSharper disable All

namespace DynamitTester
{
    public class Program
    {
        static void Main()
        {
            #region Initial boring stuff

            DynamitConfig.Init(enableEscapeStrings: true);
            foreach (var x in Db.SQL<DDictionary>($"SELECT t FROM {typeof(DDictionary).FullName} t"))
                Db.TransactAsync(() => x.Delete());
            foreach (var x in Db.SQL<DList>($"SELECT t FROM {typeof(DList).FullName} t"))
                Db.TransactAsync(() => x.Delete());

            #endregion

            // TEST BEGIN

            #region Inserting and removing values of various types

            MyDict myDict = null;
            Db.TransactAsync(() =>
            {
                myDict = new MyDict();
                dynamic d = myDict;
                d.Id = 1;
                d.Bool = true;
                d.Byte = (byte) 125;
                d.DateTime = DateTime.Now;
                d.Decimal = 123.123123123123M;
                d.Double = 123.123321123321123321;
                d.Int = 124421;
                d.Long = 33219239321L;
                d.Sbyte = (sbyte) 123;
                d.Short = (short) 332;
                d.Single = (float) 123.1;
                d.String = "\"ASddsddsa\"";
                d.Uint = (uint) 123321;
                d.Ulong = (ulong) 123321123;
                d.Ushort = (ushort) 12331;
                d.Null = null;
                d.Thing = "SwooBoo";

                new MyDict
                {
                    ["Id"] = 2,
                    ["Byte"] = (byte) 12,
                    ["String"] = "A",
                    ["Bool"] = true,
                    ["Goo"] = 123
                };

                new MyDict
                {
                    Foo = "TheThird",
                    ["Id"] = 3,
                    ["Byte"] = (byte) 122,
                    ["String"] = "A",
                    ["Bool"] = true
                };
            });

            Assert(!myDict.ContainsKey("Null"));
            Assert(myDict.ContainsKey("Thing"));
            Db.TransactAsync(() => myDict["Thing"] = null);
            Assert(!myDict.ContainsKey("Thing"));

            #endregion

            #region Checking that things exist

            Assert(Db.SQL<DDictionary>("SELECT t FROM Dynamit.DDictionary t").Any());
            Assert(Db.SQL<DKeyValuePair>("SELECT t FROM Dynamit.DKeyValuePair t").Any());
            Assert(Db.SQL<ValueObject>("SELECT t FROM Dynamit.ValueObjects.ValueObject t").Any());

            #endregion

            #region Type consistency assertions

            dynamic dyn = myDict;
            Assert(dyn.Id is int);
            Assert(dyn.Bool is bool);
            Assert(dyn.Byte is byte);
            Assert(dyn.DateTime is DateTime);
            Assert(dyn.Decimal is Decimal);
            Assert(dyn.Double is double);
            Assert(dyn.Int is int);
            Assert(dyn.Long is long);
            Assert(dyn.Sbyte is sbyte);
            Assert(dyn.Short is short);
            Assert(dyn.Single is float);
            Assert(dyn.String is string);
            Assert(dyn.Uint is uint);
            Assert(dyn.Ulong is ulong);
            Assert(dyn.Ushort is ushort);
            Assert(dyn.Null is null);

            #endregion

            #region DDictionary instance member checks

            Assert(myDict.KvpTable == typeof(MyDictKVP).FullName);
            Db.TransactAsync(() => myDict.Add("Test1", "Swoo"));
            Assert(myDict.TryGetValue("Test1", out var t01) && t01.Equals("Swoo"));
            var test2kvp = new KeyValuePair<string, object>("Test2", "Goo");
            Db.TransactAsync(() => myDict.Add(test2kvp));
            Assert(myDict.TryGetValue("Test2", out var t02) && t02.Equals("Goo"));
            Assert(myDict.Contains(test2kvp));
            Assert(myDict.Count == myDict.KeyValuePairs.Count());
            Assert(myDict.ContainsKey("Test2"));
            Db.TransactAsync(() => myDict.Remove("Test1"));
            Assert(!myDict.TryGetValue("Test1", out t01));
            Db.TransactAsync(() => myDict.Remove("Test1"));
            Assert(!myDict.TryGetValue("Test1", out t01));
            Db.TransactAsync(() => myDict.Remove(test2kvp.Key));
            Assert(!myDict.TryGetValue("Test2", out t01));
            var arr = new KeyValuePair<string, object>[100];
            myDict.CopyTo(arr, 0);
            Assert(arr[0].Key != null);
            Assert(arr.Count(a => a.Key != null) == myDict.KeyValuePairs.Count());

            MyDict testThingy = null;
            Db.TransactAsync(() => testThingy = new MyDict {["Test"] = true});
            Assert(testThingy.Any());
            Db.TransactAsync(() => testThingy.Clear());
            Assert(!testThingy.Any());
            Db.TransactAsync(() => testThingy.Delete());

            object thing = myDict.SafeGet("Swoofooboasd");
            Assert(thing == null);
            var otherThing = myDict.SafeGet("Bool");
            Assert(otherThing is bool);

            #endregion

            #region Finder

            var all = Finder<MyDict>.All;
            var As = Finder<MyDict>.Where("String", EQUALS, "A");
            var third = Finder<MyDict>.Where("Id", EQUALS, 3);
            var firstAndSecond = Finder<MyDict>.All.Where(dict => dict["Id"] < 3);
            var second = Finder<MyDict>.Where(("String", EQUALS, "A"), ("Byte", NOT_EQUALS, 122));
            var alsoThird = Finder<MyDict>.Where(("String", EQUALS, "A")).Where(dict => dict["Byte"] == 122).FirstOrDefault();
            var alsoThird3 = Finder<MyDict>.Where("$Foo", EQUALS, "TheThird").FirstOrDefault();
            Assert(alsoThird.Equals(alsoThird3));

            var objectNo = myDict.GetObjectNo();
            var objectId = myDict.GetObjectID();
            var _first = Finder<MyDict>.Where("$ObjectID", EQUALS, objectId).FirstOrDefault();
            var _first2 = Finder<MyDict>.Where("$ObjectNo", EQUALS, objectNo).FirstOrDefault();
            Assert(_first.Equals(_first2) && _first.Equals(myDict));

            var firstAndThird = Finder<MyDict>.Where("Goo", EQUALS, null);
            var alsoSecond = Finder<MyDict>.Where("Goo", NOT_EQUALS, null);

            var json = JsonConvert.SerializeObject(all);

            var alsoAll1 = Finder<MyDict>.Where();
            var alsoAll2 = Finder<MyDict>.Where(("Gooo", EQUALS, null));
            var alsoAll3 = Finder<MyDict>.Where(("Gooo", EQUALS, null));
            Assert(alsoAll1.Count() == 3);
            Assert(alsoAll2.Count() == 3);
            Assert(alsoAll3.Count() == 3);

            Assert(all.Count() == 3);
            Assert(As.Count() == 2);
            bool thirdOk = third.Count() == 1 && third.First()["Id"] == 3;
            Assert(thirdOk);
            var firstAndSecondOk = firstAndSecond.Count() == 2;
            Assert(firstAndSecondOk);
            bool secondOk = second.Count() == 1 && second.First()["Id"] == 2;
            Assert(secondOk);
            bool alsoThirdOk = third.Count() == 1 && third.First()["Id"] == 3;
            Assert(alsoThirdOk);

            Assert(firstAndThird.Count() == 2);
            bool alsoSecondOk = alsoSecond.Count() == 1 && alsoSecond.First()["Id"] == 2;
            Assert(alsoSecondOk);

            #endregion

            #region Handling errors in input

            Db.TransactAsync(() =>
            {
                try
                {
                    myDict["T"] = new {A = "ASD"};
                }
                catch (Exception e)
                {
                    Assert(e is InvalidValueTypeException);
                }
                try
                {
                    myDict["V"] = new Stopwatch();
                }
                catch (Exception e)
                {
                    Assert(e is InvalidValueTypeException);
                }
                try
                {
                    myDict["X"] = new JObject {["A"] = 123};
                }
                catch (Exception e)
                {
                    Assert(e is InvalidValueTypeException);
                }
            });
            Assert(!myDict.ContainsKey("T"));
            Assert(!myDict.ContainsKey("V"));
            Assert(!myDict.ContainsKey("X"));

            #endregion

            #region JSON serialization

            var asJson = JsonConvert.SerializeObject(myDict);

            #endregion

            #region Deleting rows

            Db.TransactAsync(() =>
            {
                myDict.Delete();
                second.First().Delete();
                third.First().Delete();
            });

            #endregion

            #region Checking that things don't exist anymore

            Assert(!Db.SQL<DDictionary>("SELECT t FROM Dynamit.DDictionary t").Any());
            Assert(!Db.SQL<DKeyValuePair>("SELECT t FROM Dynamit.DKeyValuePair t").Any());
            Assert(!Db.SQL<ValueObject>("SELECT t FROM Dynamit.ValueObjects.ValueObject t").Any());

            #endregion

            // TEST END

            var s = 0;
        }
    }


    public class MyDict : DDictionary, IDDictionary<MyDict, MyDictKVP>
    {
        public string Foo;

        public MyDictKVP NewKeyPair(MyDict dict, string key, object value = null)
        {
            return new MyDictKVP(dict, key, value);
        }
    }

    public class MyDictKVP : DKeyValuePair
    {
        public MyDictKVP(DDictionary dict, string key, object value = null) : base(dict, key, value) { }
    }
}