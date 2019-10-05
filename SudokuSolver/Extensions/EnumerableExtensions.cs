using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace SudokuSolver.Extensions
{
    public static class EnumerableExtensions
    {
        public static Dictionary<TKey, IEnumerable<TValue>> ToDictionaryGrouping<TKey, TValue>(this IEnumerable<TValue> list, Func<TValue, TKey> keySelect)
            => list.GroupBy(p => keySelect.Invoke(p)).ToDictionary();
        public static Dictionary<TKey, IEnumerable<TValueOut>> ToDictionaryGrouping<TKey, TValueIn, TValueOut>(this IEnumerable<TValueIn> list, Func<TValueIn, TKey> keySelect, Func<TValueIn, TValueOut> valueSelect)
            => list.GroupBy(p => keySelect.Invoke(p), p => valueSelect.Invoke(p)).ToDictionary();
        public static Dictionary<TKey, IEnumerable<TValue>> ToDictionaryGrouping<TKey, TValue>(this IEnumerable<TValue> list, Func<TValue, TKey> keySelect, IEqualityComparer<TKey> comparer)
            => list.GroupBy(p => keySelect.Invoke(p), comparer).ToDictionary(comparer);
        public static Dictionary<TKey, IEnumerable<TValueOut>> ToDictionaryGrouping<TKey, TValueIn, TValueOut>(this IEnumerable<TValueIn> list, Func<TValueIn, TKey> keySelect, Func<TValueIn, TValueOut> valueSelect, IEqualityComparer<TKey> comparer)
            => list.GroupBy(p => keySelect.Invoke(p), p => valueSelect.Invoke(p), comparer).ToDictionary(comparer);
        public static Dictionary<TKey, IEnumerable<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groups)
            => groups.ToDictionary(p => p.Key, p => p.AsEnumerable());
        public static Dictionary<TKey, IEnumerable<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groups, IEqualityComparer<TKey> comparer)
            => groups.ToDictionary(p => p.Key, p => p.AsEnumerable(), comparer);

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> list)
            => list.SelectMany(p => p);
        public static IEnumerable<T> Select<T>(this IEnumerable<T> list)
            => list.Select(p => p);

        public static void RemoveAll<T>(this List<T> list) => list.RemoveAll(p => true);

        public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> list)
            => list.Select((item, index) => (index, item));

        public static IEnumerable<T> WhereIs<T>(this IEnumerable<T> list, T value, IEqualityComparer<T> comparer = null)
        {
            if (comparer is null) comparer = EqualityComparer<T>.Default;
            return list.Where(item => comparer.Equals(item, value));
        }
        public static IEnumerable<T> WhereIs<T>(this IEnumerable<T> list, params T[] values)
            => list.WhereIs(values.AsEnumerable());
        public static IEnumerable<T> WhereIs<T>(this IEnumerable<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer = null)
            => list.Intersect(values, comparer ?? EqualityComparer<T>.Default);

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> list, T value, IEqualityComparer<T> comparer = null)
        {
            if (comparer is null) comparer = EqualityComparer<T>.Default;
            return list.Where(item => !comparer.Equals(item, value));
        }
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> list, params T[] values)
            => list.WhereNot(values.AsEnumerable());
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer = null)
            => list.Except(values, comparer ?? EqualityComparer<T>.Default);

        public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            bool isFirst = true;
            return list.Where(p =>
            {
                if (predicate.Invoke(p) && isFirst)
                {
                    isFirst = false;
                    return false;
                }
                return true;
            });
        }

        public static IEnumerable<T> RemoveAfterFirst<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            bool isFirst = true;
            return list.Where(p =>
            {
                if (predicate.Invoke(p))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        return true;
                    }
                    return false;
                }
                return true;
            });
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
                action.Invoke(item);
        }
        public static void ForEach<T>(this IEnumerable<T> list, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in list)
            {
                action.Invoke(item, index);
                index += 1;
            }
        }

        public static void Add<TValue>(this ICollection<TValue> collection, params TValue[] values)
        {
            foreach (var value in values)
                collection.Add(value);
        }

        public static bool CountEQ<T>(this IEnumerable<T> list, int nMatches)
            => list.Take(nMatches + 1).Count() == nMatches;
        public static bool CountLT<T>(this IEnumerable<T> list, int nMatches)
            => list.Take(nMatches).Count() < nMatches;
        public static bool CountGT<T>(this IEnumerable<T> list, int nMatches)
            => list.Take(nMatches + 1).Count() > nMatches;

        public static bool Any<T>(this IEnumerable<T> list, int nMatches, Func<T, bool> predicate)
            => list.Where(predicate).Take(nMatches).Count() == nMatches;

        public static Dictionary<TKey, TValue> RemoveIfKeys<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TKey, bool> predicate)
            => dictionary.RemoveIf((k, v) => predicate.Invoke(k));
        public static Dictionary<TKey, TValue> RemoveIfValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TValue, bool> predicate)
            => dictionary.RemoveIf((k, v) => predicate.Invoke(v));
        public static Dictionary<TKey, TValue> RemoveIf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate)
        {
            foreach (var key in dictionary.Keys)
                if (!predicate.Invoke(key, dictionary[key]))
                    dictionary.Remove(key);
            return dictionary;
        }

        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            => GetValueOrAdd(dictionary, key, default(TValue));
        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
            => GetValueOrAdd(dictionary, key, () => defaultValue);
        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> supplier)
            => GetValueOrAdd(dictionary, key, (k) => supplier.Invoke());
        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> supplier)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;
            value = supplier.Invoke(key);
            dictionary.Add(key, value);
            return value;
        }

        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, TValue value, params TKey[] keys) where TDict : IDictionary<TKey, TValue>
            => dictionary.Fill(value, keys.ToList());
        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, TValue value, IEnumerable<TKey> keys) where TDict : IDictionary<TKey, TValue>
            => dictionary.Fill((k) => value, keys.ToList());
        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, Func<TValue> valueSupplier, params TKey[] keys) where TDict : IDictionary<TKey, TValue>
            => dictionary.Fill(valueSupplier, keys.ToList());
        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, Func<TValue> valueSupplier, IEnumerable<TKey> keys) where TDict : IDictionary<TKey, TValue>
            => dictionary.Fill((k) => valueSupplier.Invoke(), keys.ToList());
        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, Func<TKey, TValue> valueSupplier, params TKey[] keys) where TDict : IDictionary<TKey, TValue>
            => dictionary.Fill(valueSupplier, keys.ToList());
        public static TDict Fill<TDict, TKey, TValue>(this TDict dictionary, Func<TKey, TValue> valueSupplier, IEnumerable<TKey> keys) where TDict : IDictionary<TKey, TValue>
        {
            foreach (var key in keys)
                dictionary.GetValueOrAdd(key, valueSupplier);
            return dictionary;
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list, int start, int end)
            => Enumerable.Range(start, start - end).SelectMany(i => list.GetPermutations(i));
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> list, int start, int end) where T : IComparable
            => Enumerable.Range(start, end - start + 1).SelectMany(i => list.GetCombinations(i));
        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return list.GetCombinations(length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public static IEnumerable<IEnumerable<T>> GetRollingWindow<T>(this IEnumerable<T> list, int size)
            => Enumerable.Range(0, list.Count() - size + 1).Select(i => list.Skip(i).Take(size));

        public static int GetRealHashCode(this IEnumerable<object> list, int inital, int shift)
        {
            if (list is null)
                return 0;
            unchecked
            {
                int hash = inital;
                foreach (var item in list)
                    hash = hash * shift + (item is null ? 0 : item.GetHashCode());
                return hash;
            }
        }
    }
}
