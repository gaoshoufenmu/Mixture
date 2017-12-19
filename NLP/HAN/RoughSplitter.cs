using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLP.utils;

namespace NLP.HAN
{
    public class RoughSplitter
    {
        public static List<RoughChunk> RoughSplit(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var list = new List<RoughChunk>();
            var sb = new StringBuilder(input.Length);       // 缓存当前roughchunk的文本内容
            int i = 0;                  // input当前处理字符位置


            var c = CharUtil.ToHalfAngle(input[0]);
            var cur_type = CharUtil.GetType(c);
            int chunk_type = cur_type;          // 当前块字符类型
            while (i < input.Length)
            {
                var new_type = ShouldCombineRoughly(chunk_type, cur_type, i, input);
                while (new_type > 0)
                {
                    sb.Append(c);
                    chunk_type = new_type;
                    i++;
                    if (i == input.Length)
                        break;
                    c = CharUtil.ToHalfAngle(input[i]);
                    cur_type = CharUtil.GetType(c);
                    new_type = ShouldCombineRoughly(chunk_type, cur_type, i, input);
                }

                if (i == input.Length)   // 结束
                {
                    list.Add(new RoughChunk(sb.ToString(), i - sb.Length, chunk_type));
                    return list;
                }
                else
                {
                    list.Add(new RoughChunk(sb.ToString(), i - sb.Length, chunk_type));
                    sb.Clear();
                }
                chunk_type = cur_type;
            }
            return list;
        }

        /// <summary>
        /// 判断是否应该粗略结合当前字符到当前roughchunk中
        /// 对地址判断不准确，因为地址值包含中文，数字，符号，字母多种组合
        /// </summary>
        /// <param name="chunk_type">当前roughchunk的字符类型</param>
        /// <param name="cur_type">当前字符的类型</param>
        /// <param name="i">当前字符在原始文本中的位置</param>
        /// <param name="input">原始文本</param>
        /// <returns>小于0：不结合；大于0：结合后的块字符类型</returns>
        private static int ShouldCombineRoughly(int chunk_type, int cur_type, int i, string input)
        {
            var and = chunk_type & cur_type;
            if (and > 0) return chunk_type;

            if (chunk_type == CharUtil.Space_T || cur_type == CharUtil.Space_T)             // 首次遇到空格，进行切分
                return -1;

            if ((chunk_type & (CharUtil.Number_T | CharUtil.Alphbet_T)) > 0)        // 字母数字符号混合，但必须以字母或数字开头
            {
                if (cur_type == CharUtil.Alphbet_T || cur_type == CharUtil.Number_T || cur_type == CharUtil.Punctuation_T)
                    return chunk_type | cur_type;
            }

            if (cur_type == CharUtil.EmbedPunctuation_T)     // 当前字符是可嵌入符号
            {
                var curr_c = CharUtil.ToHalfAngle(input[i]);
                if (curr_c == ')' || curr_c == ']')        // 当前符号是关括号，则合并
                    return chunk_type | CharUtil.EmbedPunctuation_T;

                if (i < input.Length - 1)
                {
                    var next_type = CharUtil.GetType(CharUtil.ToHalfAngle(input[i + 1]));
                    and = chunk_type & next_type;                   // 前后符号一致，则合并
                    if (and > 0)
                        return chunk_type | CharUtil.EmbedPunctuation_T;
                }
            }
            else if (chunk_type == CharUtil.EmbedPunctuation_T)      // 当前块是可嵌入符号类型
            {
                var prev_c = CharUtil.ToHalfAngle(input[i - 1]);
                if (prev_c == '(' || prev_c == '[')              // 上一个符号是开括号
                    return cur_type | CharUtil.EmbedPunctuation_T;    // 合并
            }

            return -1;      // 不合并
        }

        public static void Recognition(List<RoughChunk> chunks, int i)
        {
            var chunk = chunks[i];
            var type = chunk.type;
            var len = chunk.text.Length;
            if (type == CharUtil.Number_T)     // 纯阿拉伯数字
            {
                // special handler
            }
            else if (type == CharUtil.Alphbet_T)     // 纯英文字母
            {
                // special handler
            }
        }
    }
}
