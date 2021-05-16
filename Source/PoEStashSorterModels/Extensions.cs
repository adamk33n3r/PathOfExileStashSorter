using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoEStashSorterModels.ExtensionMethods
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary,
             TKey key,
             TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
        {
            bool first = true;
            foreach (T value in source)
            {
                if (!first)
                {
                    yield return element;
                }
                yield return value;
                first = false;
            }
        }
        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, Func<T> elementCallback)
        {
            bool first = true;
            foreach (T value in source)
            {
                if (!first)
                {
                    yield return elementCallback();
                }
                yield return value;
                first = false;
            }
        }
    }
}
