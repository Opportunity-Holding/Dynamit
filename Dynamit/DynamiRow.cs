using System.Collections.Generic;
using System.Linq;
using Starcounter;
using System;

namespace Dynamit
{
    [Database]
    public class DynamiRow
    {
        public readonly DynamiTable Table;

        [Transient]
        public IEnumerable<DynamiAssignment> Data;

        public IEnumerable<DynamiAssignment> Make()
        {
            return Db.SQL<DynamiAssignment>($"SELECT t FROM {typeof(DynamiAssignment).FullName} t WHERE t.Row ?=", this);
        }

        private dynamic this[string key] => Get(key);

        public dynamic Get(string key)
        {
            dynamic result = DbHelper.FromID((Data ?? (Data = Make())).First(a => a.Key == key).ContentNo);
            return result.content;
        }

        public DynamiRow(DynamiTable table, params DynamiAssignment[] assignments)
        {
            Table = table;
        }
    }

    [Database]
    public class DynamiTable
    {
        public string Name;

        public DynamiTable(IDictionary<string, Type> schema)
        {
        }

        public bool Insert(object row)
        {
            try
            {
                new DynamiRow(this);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}