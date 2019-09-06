using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foundation.algorithm
{
    public class Algebra
    {
        /// <summary>
        /// Greatest common divisor
        /// </summary>
        public static int GCD(int x, int y)
        {
            return y == 0 ? x : GCD(y, x % y);
        }

        public static int GCD2(int x, int y)
        {
            if (x == y)
                return x;
            else if (x > y)
                x = x - y;
            else
                y = y - x;

            return GCD2(x, y);
        }

        /// <summary>
        /// Lowest common multiple
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int LCM(int x, int y)
        {
            return x * y / GCD(x, y);
        }

        /// <summary>
        /// Get all prime factors of the given number
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IEnumerable<long> PrimeFactors(long x)
        {
            int f = 2;
            while (f * f <= x)
            {
                while (x % f == 0)
                {
                    x /= f;
                    yield return f;
                }

                f++;

            }
            if (x > 1)
                yield return x;
        }
    }
}
