using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP.utils
{
    /// <summary>
    /// 字符串处理工具类
    /// </summary>
    public class StrUtil
    {
        // slide of fix-unit-length version
        private const long fm = 0x3F;
        /// <summary>
        /// Convert alpha or numberic string to long number
        /// fix-unit-length version (6 bits)
        /// </summary>
        /// <param name="input">string length is not larger than 10</param>
        /// <returns></returns>
        public static long Alnum2Long_F(string input)
        {
            long v = 0;
            long sum = 0;
            for(int i = 0; i < input.Length; ++i)
            {
                var c = input[i];
                if (c < 'A')
                    v = c - '0' + 1;        // 0 - 9
                else if (c < 'a')
                    v = c - 'A' + 11;       // A - Z
                else
                    v = c - 'a' + 37;       // a - z

                sum |= (v & fm) << (i * 6);
            }
            return sum;
        }
        /// <summary>
        /// Convert long number to alpha or numberic string(length is not large than 10)
        /// fix-unit-length version (6 bits)
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static string Long2Alnum_F(long l)
        {
            var cs = new List<char>(10);
            for(int i = 0; i < 10; ++i)
            {
                var v = (int)((l & (fm << (i * 6))) >> (i * 6));
                if (v > 0)
                {
                    if (v <= 10)
                        cs.Add((char)(v - 1 + '0'));
                    else if (v <= 36)
                        cs.Add((char)(v - 11 + 'A'));
                    else
                        cs.Add((char)(v - 37 + 'a'));
                }
                else break;
            }
            return new string(cs.ToArray());
        }

        // slide of variable-unit-length version
        private const long vm = 0xF;
        /// <summary>
        /// if char is in [0-9|A-E], then use 5 bits to represent it, else if char is in [F-Z|a-z], then use 7 bits to represent it
        /// From right to left, the fifth bit is a flag, if it is 1, then 7 bits, otherwise, 5 bits<p></p>
        /// max-length supremum is 12 when every char is in [0-9|A-E], and infimum is 9 when every char is in [F-Z|a-z].
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long Alnum2Long_V(string input)
        {
            long v = 0;
            int uc = 0;             // 占用两个单位长度的个数
            long sum = 0;
            for(int i = 0; i < input.Length; ++i)
            {
                var c = input[i];
                if (c <= '9')
                    v = ((long)(c - '0' + 1)) << (i * 5 + uc * 2);
                else if (c <= 'E')
                    v = ((long)(c - 'A' + 11)) << (i * 5 + uc * 2);
                else
                {
                    if (c <= 'Z')
                        v = (long)(c - 'A' + 11);
                    else
                        v = (long)(c - 'a' + 37);

                    var low = v & vm;
                    var high = v >> 4;
                    v = ((high << 5) | (0x10 | low)) << (i * 5 + uc * 2);
                    uc++;
                }
                sum |= v;
            }
            return sum;
        }


        public static string Long2Alnum_V(long l)
        {
            int uc = 0;
            var cs = new List<char>(10);
            for(int i = 0; i < 12; ++i)
            {
                var v = (l >> (i * 5 + uc * 2)) & 0x1F;
                if ((v & 0x10) != 0)         // [F-A|a-z]
                {
                    var low = v & vm;
                    var high = (l >> ((i + 1) * 5 + uc * 2)) & 0x3;
                    v = (high << 4) | low;
                    if (v <= 36)
                        cs.Add((char)(v - 11 + 'A'));
                    else
                        cs.Add((char)(v - 37 + 'a'));

                    uc++;
                }
                else if (v > 0)
                {
                    if (v <= 10)
                        cs.Add((char)(v - 1 + '0'));
                    else
                        cs.Add((char)(v - 11 + 'A'));
                }
                else break;
            }
            return new string(cs.ToArray());
        }

        public static long Alnum2Long_BaseX(string input, out long @base)
        {
            long sum = 0;
            var arr = new int[input.Length];
            long b = 11;
            for(int i = 0; i < input.Length; ++i)
            {
                if (input[i] <= '9')
                    arr[i] = input[i] - '0' + 1;
                else if(input[i] <= 'Z')
                {
                    arr[i] = input[i] - 'A' + 11;
                    if (b < 37)
                        b = 37;
                }
                else
                {
                    arr[i] = input[i] - 'a' + 37;
                    b = 63;
                }
            }
            for (int i = 0; i < arr.Length; ++i)
                sum += arr[i] * (b ^ i);

            @base = b;
            return sum;
        }

        public static string Long2Alnum_BaseX(long l, long b)
        {
            var cs = new List<char>(10);
            long rem = 0;
            long denum = b;
            while(true)
            {
                l -= rem;
                var r = l % denum;
                rem += r * denum;
                denum *= b;

                if (r > 0)
                {
                    if (r < 11)
                        cs.Add((char)(r - 1 + '0'));
                    else if (r < 37)
                        cs.Add((char)(r - 11 + 'A'));
                    else
                        cs.Add((char)(r - 37 + 'a'));
                }
                else break;
            }
            return new string(cs.ToArray());
        }
    }
}
