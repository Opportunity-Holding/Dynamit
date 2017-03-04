using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
//    [Database]
//    public abstract class DList : IList<object>
//    {
//        public int HighestIndex { get; private set; }
//
//        public string ElementTable { get; }
//
//        public IEnumerable<DElement> List =>
//            Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =?", this);
//
//        public DList()
//        {
//            ElementTable = GetType().GetAttribute<DListAttribute>().ElementTable.FullName;
//            HighestIndex = -1;
//        }
//
//        protected abstract DElement NewElement(DList list, int index, object value = null);
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
//            if (item == null) return;
//            var newElement = NewElement(this, HighestIndex + 1, item);
//            HighestIndex = newElement.Index;
//        }
//
//        public void Clear()
//        {
//            foreach (var element in List)
//            {
//                element.Clear();
//                element.Delete();
//            }
//        }
//
//        public bool Contains(object item)
//        {
//            return Db.SQL<DElement>(
//                       $"SELECT t FROM {ElementTable} t " +
//                       "WHERE t.List =? AND t.ValueHash =?", this, item.GetHashCode()
//                   ).First != null;
//        }
//
//        public void CopyTo(object[] array, int arrayIndex)
//        {
//            if (array == null) throw new ArgumentNullException(nameof(array));
//            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
//            if (arrayIndex + Count > array.Length - 1) throw new ArgumentException(nameof(arrayIndex));
//            foreach (var element in List)
//            {
//                array[arrayIndex] = element;
//                arrayIndex += 1;
//            }
//        }
//
//        public bool Remove(object item)
//        {
//            try
//            {
//                var obj = DB.Get<DElement>("List", this, "ValueHash", item.GetHashCode());
//                obj?.Delete();
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }
//
//        public int Count => HighestIndex + 1;
//
//        public bool IsReadOnly => false;
//
//        public int IndexOf(object item)
//        {
//            throw new NotImplementedException();
//        }
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