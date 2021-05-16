using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace PoEStashSorter
{

    public class TextCollection : IList<Inline>
    {
        private readonly InlineCollection collection;

        public event Action ContentChanged;

        public int Count => this.collection.Count;

        public bool IsReadOnly => this.collection.IsReadOnly;

        public Inline this[int index] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public TextCollection(InlineCollection collection)
        {
            this.collection = collection;
        }

        public IEnumerator<Inline> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        public int IndexOf(Inline item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, Inline item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public bool Contains(Inline item)
        {
            return this.collection.Contains(item);
        }

        public void CopyTo(Inline[] array, int arrayIndex)
        {
            this.collection.CopyTo(array, arrayIndex);
            ContentChanged?.Invoke();
        }

        public TextCollection Add(Inline item)
        {
            this.collection.Add(item);
            ContentChanged?.Invoke();
            return this;
        }

        public TextCollection AddRange(IEnumerable<Inline> range)
        {
            foreach (var item in range)
            {
                this.collection.Add(item);
            }
            ContentChanged?.Invoke();
            return this;
        }

        public bool Remove(Inline item)
        {
            var removed = this.collection.Remove(item);
            ContentChanged?.Invoke();
            return removed;
        }

        public void Clear()
        {
            this.collection.Clear();
            ContentChanged?.Invoke();
        }

        void ICollection<Inline>.Add(Inline item)
        {
            throw new NotImplementedException();
        }

        //public static implicit operator TextCollection(InlineCollection c) => (TextCollection)c.ToList();
        public static implicit operator InlineCollection(TextCollection t) => t.collection;
        public static TextCollection operator +(TextCollection collection, string text)
        {
            if (text == string.Empty)
            {
                return collection;
            }
            if (collection.Count > 0)
            {
                collection.Add(new LineBreak());
            }
            return collection + new Run(text);
        }

        public static TextCollection operator +(TextCollection collection, Inline inline)
        {
            collection.Add(inline);
            return collection;
        }

        public static TextCollection operator +(TextCollection collection, IEnumerable<Inline> other)
        {
            foreach (var ele in other)
            {
                collection += ele;
            }
            return collection;
        }
    }
}
