using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace combinator
{
    public class RecursiveLambda
    {
        //Func<int, int> fac_0 = x => x <= 1 ? 1 : x * fac_0(x - 1);

        #region pseudo_0
        Func<int, int> fac = null;
        public void fac_Init() => _fac = x => x <= 1 ? 1 : x * _fac(x - 1);
        #endregion

        #region pseudo_1
        delegate int fac_1(int i);
        fac_1 _fac;
        public void _fac_Init() => _fac = x => x <= 1 ? 1 : x * _fac(x - 1);
        #endregion

        #region invoke
        public void fac_invoke() => Console.WriteLine(fac(5));

        public void fac_invoke_1()
        {
            Func<int, int> fac_alias = fac;
            fac = x => x;
            Console.WriteLine(fac(5));
        }
        #endregion

        #region YFix
        Func<int, int> y_fac = Combinator.YFix<int, int>(f => x => x <= 1 ? 1 : x * f(x - 1));


        public void y_fac_invoke()
        {
            Console.WriteLine(y_fac(5));
        }

        Func<int, int, int> y_gcd = Combinator.YFix<int, int, int>(f => (x, y) => y == 0 ? x : f(y, x % y));
        public void y_gcd_invoke() => Console.WriteLine(y_gcd((int)Math.Pow(2, 10), 2 * 2 * 2 * 2 * 2 * 3 * 5 * 7 * 13));
        #endregion
    }

    class Combinator
    {
        // Fix(f) = f(Fix(f))       
        public static Func<T, V> YFix<T, V>(Func<Func<T, V>, Func<T, V>> f) => x => f(YFix(f))(x);

        public static Func<T, U, V> YFix<T, U, V>(Func<Func<T, U, V>, Func<T, U, V>> f) => (x, y) => f(f(YFix(f)))(x, y);
    }

    public class sorter
    {
        public static IEnumerable<T> FastSort<T>(IEnumerable<T> source) where T : IComparable =>
            Combinator.YFix<IEnumerable<T>, IEnumerable<T>>(f =>
                src => src.Any()
                    ? f(src.Skip(1).Where(little => src.First().CompareTo(little) > 0))
                        .Concat(Enumerable.Repeat(src.First(), 1))
                        .Concat(f(src.Skip(1).Where(big => src.First().CompareTo(big) < 0)))
                    : Enumerable.Empty<T>())
            (source);


        public static void Test()
        {
            int[] source = { 3, 4, 2, 8, 1, 9, 7, 5 };
            var res = FastSort(source);
            foreach (var i in res)
                Console.WriteLine(i);
        }
    }
}
