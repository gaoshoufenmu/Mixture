using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP.utils
{
    public class ConvertUtil
    {
        public static string ToHexStr(byte[] bs)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
                sb.Append(bs[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
