using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace distribution.mapreduce
{
    public static class MRExtension
    {
        /// <summary>
        /// Divides an enumerable into equal parts and perform an action on those parts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">item set</param>
        /// <param name="parts">how many parts to partition this set</param>
        /// <param name="action"></param>
        public static void Partition<T>(this IEnumerable<T> enumerable, int parts, Action<IEnumerable<T>, int, int> action)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            if (action == null)
                throw new ArgumentNullException("action");

            var actions = new List<Action>();

            if (parts <= 0)
                parts = 1;

            int count = enumerable.Count();
            int itemsPerPart = count / parts;

            if (itemsPerPart == 0)
                itemsPerPart = 1;

            for (int i = 0; i < parts; i++)
            {
                var collection = enumerable.Skip(i * itemsPerPart).Take(i == parts - 1 ? count : itemsPerPart);

                int j = i; // here it need to declare a new variable for the closure
                actions.Add(() => action(collection, j, itemsPerPart));
            }
            Parallel.Invoke(actions.ToArray());
        }

        public static void Partition<T>(this IEnumerable<T> enumerable, int parts, Action<IEnumerable<T>> action) =>
            Partition(enumerable, parts, (subset, i, j) => action(subset));


        public static void Add<T, U, V>(this IDictionary<U, List<V>> dictionary, IEnumerable<T> list, Func<T, U> keySelector, Func<T, V> valueSelector)
        {
            lock (dictionary)
            {
                foreach (var l in list)
                {
                    var key = keySelector(l);
                    List<V> val;
                    if(dictionary.TryGetValue(key, out val))
                    {
                        val.Add(valueSelector(l));
                    }
                    else
                        dictionary[key] = new List<V>() { valueSelector(l) };
                }
            }
        }

        public static void Add<T, U, V>(this IDictionary<U, V> dictionary, T t, Func<T, U> keySelector, Func<T, V> valueSelector)
        {
            lock(dictionary)
            {
                var key = keySelector(t);

                if (!dictionary.ContainsKey(key))
                    dictionary[key] = valueSelector(t);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}
