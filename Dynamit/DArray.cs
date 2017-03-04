using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
//    internal abstract class DArray : IList<object>
//    {
//        [Transient]
//        private List<object> _list;
//
//        public string KvpTable { get; }
//
//        public bool IsUpdated { get; set; }
//
//        public int CurrentLength;
//
//        private List<object> List
//        {
//            get
//            {
//                if (IsUpdated)
//                {
//                    var d = MakeList();
//                    Db.Transact(() => { IsUpdated = false; });
//                    return _list = d;
//                }
//                return _list ?? (_list = MakeList());
//            }
//        }
//
//        public DArray(Type keyValuePairTable)
//        {
//            CurrentLength = 0;
//            KvpTable = keyValuePairTable.FullName;
//        }
//
//        internal IEnumerable<DElement> Elements =>
//            Db.SQL<DElement>($"SELECT t FROM {KvpTable} t WHERE t.List =?", this);
//
//        private List<object> MakeList()
//        {
//            return Elements.OrderBy(e => e.Index).Select(e => e.Value).ToList();
//        }
//
//        protected abstract DElement NewElement(DArray list, object value = null);
//
//        public void Update()
//        {
//            Db.Transact(() => { IsUpdated = true; });
//        }
//
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//
//        public IEnumerator<object> GetEnumerator()
//        {
//            return List.GetEnumerator();
//        }
//
//        public void Add(object item)
//        {
//            NewElement(this, item);
//            Update();
//        }
//
//        public void Clear()
//        {
//            foreach (var pair in Elements)
//            {
//                pair.Clear();
//                pair.Delete();
//            }
//            Update();
//        }
//
//        public bool Contains(object item)
//        {
//            return List.Contains(item);
//        }
//
//        public void CopyTo(object[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool Remove(object item)
//        {
//            try
//            {
//                var obj = DB.Get<DElement>("List", this, "Value", item);
//                obj?.Delete();
//                Update();
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }
//
//        public int Count => List.Count;
//        public bool IsReadOnly => false;
//        public int IndexOf(object item) => List.IndexOf(item);
//
//        public void Insert(int index, object item)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void RemoveAt(int index)
//        {
//            throw new NotImplementedException();
//        }
//
//        public object this[int index]
//        {
//            get { throw new NotImplementedException(); }
//            set { throw new NotImplementedException(); }
//        }
//    }
}