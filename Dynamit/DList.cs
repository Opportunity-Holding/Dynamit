using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace Dynamit
{
    [Database]
    public abstract class DList : IList<object>, ICollection<object>, IReadOnlyList<object>,
        IReadOnlyCollection<object>, IEnumerable<object>, IEnumerable, IEntity
    {
        public int HighestIndex { get; private set; }

        public string ElementTable { get; }

        public IEnumerable<DElement> Elements => Db.SQL<DElement>($"SELECT t FROM {ElementTable} " +
                                                                  "t WHERE t.List =? ORDER BY t.\"Index\"", this);

        public DList()
        {
            ElementTable = GetType().GetAttribute<DListAttribute>().ElementTable.FullName;
            HighestIndex = -1;
        }

        protected abstract DElement NewElement(DList list, int index, object value = null);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        public void Add(object item)
        {
            if (item == null) return;
            var newElement = NewElement(this, HighestIndex + 1, item);
            HighestIndex = newElement.Index;
        }

        public void Clear()
        {
            foreach (var element in Elements)
            {
                element.Clear();
                element.Delete();
            }
        }

        public bool Contains(object item)
        {
            return Db.SQL<DElement>(
                           $"SELECT t FROM {ElementTable} t " +
                           "WHERE t.List =? AND t.ValueHash =?", this, item.GetHashCode())
                       .First != null;
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Count > array.Length - 1) throw new ArgumentException(nameof(arrayIndex));
            foreach (var element in Elements)
            {
                array[arrayIndex] = element;
                arrayIndex += 1;
            }
        }

        public bool Remove(object item)
        {
            try
            {
                var obj = Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =? " +
                                           "AND t.ValueHash =?", this, item.GetHashCode())
                    .First;
                if (obj == null)
                    return true;
                var index = obj.Index;
                obj.Delete();
                SetIndexes(index + 1, -1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int Count => HighestIndex + 1;

        public bool IsReadOnly { get; set; }

        public int IndexOf(object item)
        {
            if (item == null)
                return -1;
            var obj = Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =? " +
                                       "AND t.ValueHash =?", this, item.GetHashCode())
                .First;
            return obj?.Index ?? -1;
        }

        public void Insert(int index, object item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (item == null) return;
            SetIndexes(index, 1);
            NewElement(this, index, item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var obj = Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =? " +
                                       "AND t.\"Index\" =?", this, index)
                .First;
            obj?.Delete();
            SetIndexes(index + 1, -1);
        }

        public object this[int index]
        {
            get
            {
                return Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =? " +
                                        "AND t.\"Index\" =?", this, index)
                    .First;
            }
            set { Insert(index, value); }
        }

        private void SetIndexes(int fromIndex, int incrementor)
        {
            DElement last = null;
            for (var i = fromIndex; i < Count; i += 1)
            {
                last = Db.SQL<DElement>($"SELECT t FROM {ElementTable} t WHERE t.List =? " +
                                        "AND t.\"Index\" =?", this, i)
                    .First;
                last.Index += incrementor;
            }
            HighestIndex = last?.Index ?? fromIndex;
        }

        public void OnDelete()
        {
            foreach (var element in Elements)
                element.Delete();
        }
    }
}