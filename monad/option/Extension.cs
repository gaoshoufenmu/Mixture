using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace monad.option
{
    static class Extension
    {
        /// <summary>
        /// Bind
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Either<T, V> SelectMany<T, U, V>(this Either<T, U> e, Func<U, Either<T, V>> f)
        {
            // Priority to select left value
            if (e.HasLeft)
                return new Either<T, V>().SetLeft(e.Left);

            return f(e.Right);
        }

        public static Either<T, V> SelectMany<T, R, U, V>(this Either<T, R> e, Func<R, Either<T, U>> f, Func<R, U, V> s)
        {
            return e.SelectMany<T, R, V>(r => f(r).SelectMany<T, U, V>(u => new Either<T, V>().SetRight(s(r, u))));
        }

        public static void Test()
        {
            Console.WriteLine("5 + 6 + 7 is {0}",
                from x in Either<DateTime, int>.FromRight(5)
                from y in Either<DateTime, int>.FromRight(6)
                from z in Either<DateTime, int>.FromRight(7)
                select x + y + z);

            Console.WriteLine("Left-DateTime + 6 + 7 is {0}",
                from x in Either<DateTime, int>.FromLeft(DateTime.Now)
                from y in Either<DateTime, int>.FromRight(6)
                from z in Either<DateTime, int>.FromRight(7)
                select x + y + z);

            Console.WriteLine("5 + Left-DateTime + 7 is {0}",
                from x in Either<DateTime, int>.FromRight(5)
                from y in Either<DateTime, int>.FromLeft(DateTime.Now)
                from z in Either<DateTime, int>.FromRight(7)
                select x + y + z);

            Console.WriteLine("5 + 6 + Left-DateTime is {0}",
                from x in Either<DateTime, int>.FromRight(5)
                from y in Either<DateTime, int>.FromRight(6)
                from z in Either<DateTime, int>.FromLeft(DateTime.Now)
                select x + y + z);
        }


        [DebuggerStepThrough]
        public static Option<T> MaybeCast<T>(this object current)
        {
            if (current is T)
                return (T)current;

            return Option<T>.None;
        }

        [DebuggerStepThrough]
        public static Option<V> SelectMany<T, U, V>(this Option<T> o, Func<T, Option<U>> f, Func<T, U, V> s)
        {
            return o.SelectMany<V>(t => f(t).SelectMany<V>(u => s(t, u)));

            //var defaultValue = Option<V>.None;
            //if (!o.IsSome)
            //    return defaultValue;

            //var t = o.ForceValue();
            //var mu = f(t);

            //if (!mu.IsSome)
            //    return defaultValue;

            //var u = mu.ForceValue();
            //return s(t, u);
        }

        [DebuggerStepThrough]
        public static Option<T> FirstOrDefault<T>(this IEnumerable<Option<T>> options)
        {
            foreach (var option in (options ?? Enumerable.Empty<Option<T>>()).Where(o => o.IsSome))
            {
                return option;
            }

            return Option<T>.None;
        }

        [DebuggerStepThrough]
        public static Option<T> AsOption<T>(this T t)
        {
            return t;
        }

        [DebuggerStepThrough]
        public static Option<T> Flatten<T>(this Option<Option<T>> option)
        {
            return option.SelectMany(t => t);
        }

        [DebuggerStepThrough]
        public static Option<T> Then<T>(this bool predicate, Func<T> callbackForTrue)
        {
            return predicate ? callbackForTrue() : Option<T>.None;
        }

        [DebuggerStepThrough]
        public static Option<T> NoneIfNull<T>(this T t) where T : class
        {
            return t == null ? Option<T>.None : t;
        }

        [DebuggerStepThrough]
        public static Option<bool> NoneIfFalse(this bool val)
        {
            return val ? (Option<bool>)true : Option<bool>.None;
        }

        [DebuggerStepThrough]
        public static Option<T> NoneIfEmpty<T>(this T? nullable) where T : struct
        {
            return nullable.HasValue ? nullable.Value : Option<T>.None;
        }

        [DebuggerStepThrough]
        public static Option<Value> MaybeGetValue<Key, Value>(this IDictionary<Key, Value> dict, Key key)
        {
            Value v;
            if (!dict.TryGetValue(key, out v))
                return Option<Value>.None;

            return v;
        }

        [DebuggerStepThrough]
        public static Option<IEnumerable<Value>> MaybeGetValues<Key, Value>(this ILookup<Key, Value> lookup, Key key)
        {
            if (lookup.Contains(key))
                return lookup[key].AsOption();

            return Option<IEnumerable<Value>>.None;
        }

        [DebuggerStepThrough]
        public static Option<Result> IfTrueThen<Result>(this bool lookup, Func<Result> result)
        {
            return lookup ? result() : Option<Result>.None;
        }

        [DebuggerStepThrough]
        public static Option<Result> IfTrueThen<Result>(this bool lookup, Result result)
        {
            return lookup.IfTrueThen(() => result);
        }

        [DebuggerStepThrough]
        public static IEnumerable<A> ConcatOptions<A>(this IEnumerable<Option<A>> options)
        {
            return options.Where(o => o.IsSome).Select(o => o.ForceValue());
        }

        public static Maybe<U> SelectMany<T, U>(this Maybe<T> m, Func<T, Maybe<U>> f)
        {
            if (!m.HasValue)
                return Maybe<U>.Nothing;
            return f(m.Value);
        }

        public static Maybe<V> SelectMany<T, U, V>(this Maybe<T> m, Func<T, Maybe<U>> f, Func<T, U, V> s)
        {
            return m.SelectMany<T, V>(t => f(t).SelectMany<U, V>(u => s(t, u)));
        }

        public static void Test1()
        {
            // implicit conversion
            Maybe<int> five = 5;
            Maybe<int> six = 6;
            Console.WriteLine("5 + 6 is {0}",
                from x in five
                from y in six
                select x + y);
            Console.WriteLine("Nothing + 6 is {0}",
                from x in Maybe<int>.Nothing
                from y in (Maybe<int>)6
                select x + y);

            // constructor method
            Console.WriteLine("5 + Nothing is {0}",
                from x in new Maybe<int>(5)
                from y in Maybe<int>.Nothing
                select x + y);
        }
    }
}
