using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLP.utils;

namespace NLP.HAN
{
    /// <summary>
    /// 粗分后的块类
    /// </summary>
    public class RoughChunk
    {
        /// <summary>
        /// 块文本
        /// </summary>
        public string text;
        /// <summary>
        /// 块在原始文本中的起始位置
        /// </summary>
        public int offset;
        /// <summary>
        /// 块文本的字符类型
        /// <seealso cref="Consts.Other"/>~<seealso cref="Consts.Chinese"/>
        /// </summary>
        public int type;

        public RoughChunk(string text, int offset, int type = CharUtil.Other_T)
        {
            this.text = text;
            this.offset = offset;
            this.type = type;
        }

        public override string ToString() => $"{text}, {offset}, {type}";
    }
}
