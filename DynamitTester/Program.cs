using Dynamit;
using System;
using System.Diagnostics;
using System.Linq;
using Dynamit.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Starcounter;

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

            MyDict createdDict = null;
            Db.TransactAsync(() =>
            {
                createdDict = new MyDict();
                dynamic d = createdDict;
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
            });

            Debug.Assert(!createdDict.ContainsKey("Null"));
            Debug.Assert(createdDict.ContainsKey("Thing"));
            Db.TransactAsync(() => createdDict["Thing"] = null);
            Debug.Assert(!createdDict.ContainsKey("Thing"));

            #endregion

            #region Checking that things exist

            Debug.Assert(Db.SQL<DDictionary>("SELECT t FROM Dynamit.DDictionary t").Any());
            Debug.Assert(Db.SQL<DKeyValuePair>("SELECT t FROM Dynamit.DKeyValuePair t").Any());
            Debug.Assert(Db.SQL<ValueObject>("SELECT t FROM Dynamit.ValueObjects.ValueObject t").Any());

            #endregion

            #region Handling errors in input

            Db.TransactAsync(() =>
            {
                try
                {
                    createdDict["T"] = new {A = "ASD"};
                }
                catch (Exception e)
                {
                    Debug.Assert(e is InvalidValueTypeException);
                }
                try
                {
                    createdDict["V"] = new Stopwatch();
                }
                catch (Exception e)
                {
                    Debug.Assert(e is InvalidValueTypeException);
                }
                try
                {
                    createdDict["X"] = new JObject {["A"] = 123};
                }
                catch (Exception e)
                {
                    Debug.Assert(e is InvalidValueTypeException);
                }
            });
            Debug.Assert(!createdDict.ContainsKey("T"));
            Debug.Assert(!createdDict.ContainsKey("V"));
            Debug.Assert(!createdDict.ContainsKey("X"));

            #endregion

            #region JSON serialization

            var asJson = JsonConvert.SerializeObject(createdDict);

            #endregion

            #region Deleting rows

            Db.TransactAsync(() => createdDict.Delete());

            #endregion

            #region Checking that things don't exist anymore

            Debug.Assert(!Db.SQL<DDictionary>("SELECT t FROM Dynamit.DDictionary t").Any());
            Debug.Assert(!Db.SQL<DKeyValuePair>("SELECT t FROM Dynamit.DKeyValuePair t").Any());
            Debug.Assert(!Db.SQL<ValueObject>("SELECT t FROM Dynamit.ValueObjects.ValueObject t").Any());

            #endregion

            // TEST END

            var s = 0;
        }
    }

    public class MyDict : DDictionary, IDDictionary<MyDict, MyDictKVP>
    {
        public MyDictKVP NewKeyPair(MyDict dict, string key, object value = null)
        {
            return new MyDictKVP(dict, key, value);
        }
    }

    public class MyDictKVP : DKeyValuePair
    {
        public MyDictKVP(DDictionary dict, string key, object value = null) : base(dict, key, value)
        {
        }
    }
}