using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP.utils
{
    /// <summary>
    /// 字符处理工具类
    /// </summary>
    public class CharUtil
    {
        public const string EmbedPuncts = "-()[]";
        /// <summary>
        /// 可嵌入的符号"-()[]"
        /// </summary>
        public const int EmbedPunctuation_T = 1;
        /// <summary>
        /// 其余符号
        /// </summary>
        public const int Punctuation_T = 2;
        /// <summary>
        /// 空格" \t"
        /// </summary>
        public const int Space_T = 4;



        /// <summary>
        /// 数字类型
        /// </summary>
        public const int Number_T = 128;
        /// <summary>
        /// 字母
        /// </summary>
        public const int Alphbet_T = 256;

        /// <summary>
        /// 中文字符类型
        /// </summary>
        public const int Chinese_T = 1024;
        /// <summary>
        /// 其他类型
        /// </summary>
        public const int Other_T = 0;

        // ------------------------ 领域专用类型 --------------------------
        /// <summary>
        /// 日期分隔符"/-:., "    ->    1.标点符号类型 2.空格类型
        /// </summary>
        public const int DateSep_T = 5;

        /// <summary>
        /// 空格，字母（大小写），数字，以及常见标点符号： 全角转半角
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ToHalfAngle(char c)
        {
            if (c == 12288)
                return (char)32;
            if (c > 65280 && c < 65375)
                return (char)(c - 65248);

            if (c == '【')
                c = '[';
            else if (c == '】')
                c = ']';
            else if (c == '—')
                return '-';
            else if (c == '。')
                c = '.';
            else if (c == '〇')
                return '0';

            //else if (c == '（')           // 65288
            //    c = '(';
            //else if (c == '）')      // 65289
            //    c = ')';
            //else if (c == '：')    // 65306
            //    c = ':';
            //else if (c == '，')      // 65292
            //    c = ',';
            return c;
        }

        public static bool IsLeftBracket(char c) => "\"‘“([{<⦅〈《「『【〔〖〘〚〝︵︷︹︻︽︿﹁﹃﹙﹛﹝（［｛｟｢".Contains(c);
        public static bool IsRightBracket(char c) => "\"’”)]}>⦆〉》」』】〕〗〙〛〞︶︸︺︼︾﹀﹂﹄﹚﹜﹞）］｝｠｣".Contains(c);
        public static bool IsEndSymbol(char c) => @".!?。！？,，、:：；;".Contains(c);
        public static bool IsSpace(char c) => c == ' ' || c == '　' || c == '\r' || c == '\n' || c == '\t' || c == '\f';

        /// <summary>
        /// 获取字符基本类型
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetType(char c)
        {
            if (c == ' ' || c == '\t') return Space_T;
            if (c >= '0' && c <= '9') return Number_T;
            if (c >= 'a' && c <= 'z') return Alphbet_T;
            if (c >= 'A' && c <= 'Z') return Alphbet_T;
            if (c >= '\u4e00' && c <= '\u9fa5') return Chinese_T;
            if (EmbedPuncts.Contains(c)) return EmbedPunctuation_T;
            //if (char.IsPunctuation(c)) return Consts.Punctuation;
            //return Consts.Other;
            return Punctuation_T;
        }
    }
}
