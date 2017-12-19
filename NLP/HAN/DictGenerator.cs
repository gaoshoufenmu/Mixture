using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP.HAN
{
    /// <summary>
    /// 中文词典生成器
    /// 根据互信息和熵从文本中获取词条
    /// 适用于无监督模式，新词发现
    /// </summary>
    public class DictGenerator
    {
        private static Dictionary<string, Word> _dict;
        public static Dictionary<string, Word> Dict { get; private set; } = new Dictionary<string, Word>();

        /// <summary>
        /// 互信息筛选下限，与<see cref="_total_count"/>根据经验确定，try-and-modify
        /// </summary>
        public static double Mi_Floor { get; set; } = 500;
        /// <summary>
        /// 信息熵筛选下限，根据经验确定，try-and-modify
        /// </summary>
        public static double Entropy_Floor { get; set; } = 0.5;

        /// <summary>
        /// 所有词总量
        /// </summary>
        private static int _total_count;
        private static int _maxwordlen = 4;
        /// <summary>
        /// 最大词长
        /// </summary>
        public static int MaxWordLen { get => _maxwordlen; set => _maxwordlen = value; }

        /// <summary>
        /// 统计指定文本
        /// </summary>
        /// <param name="text"></param>
        public static void Statistic(string text)
        {
            int l = text.Length;
            for(int i = 0; i < l; ++i)
            {
                // j -> 词的长度， i + j - 1 -> 词的结束位置
                for (int j = 1; i+j <= l && j <= MaxWordLen; ++j)
                {
                    var w = text.Substring(i, j);
                    if (_dict.TryGetValue(w, out Word word))
                        word.freq++;
                    else
                        _dict[w] = new Word(w, 1);

                    if (j > 1 && i > 0)
                        _dict[w].Add_Char(text[i - 1], true);
                    if (j > 1 && i + j < l)
                        _dict[w].Add_Char(text[i + j], false);

                    ++_total_count;
                }
            }
        }

        /// <summary>
        /// 统计文本结束后，更新各词条的互信息和熵
        /// </summary>
        public static void Update_Mi_Ent()
        {
            foreach(var p in _dict)
            {
                p.Value.Update_Entropy(_total_count);
                Update_Mi(p.Value);
            }
        }

        /// <summary>
        /// 更新词条的互信息和熵之后，再进行筛选（否则词条越来越大）
        /// </summary>
        public static void Filter()
        {
            foreach(var p in _dict)
            {
                if (p.Value.mi > Mi_Floor && p.Value.entropy > Entropy_Floor)
                    Dict.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// 更新互信息
        /// Mi(x,y) = log[p(x,y)/(p(x)p(y))]
        /// 分词后再根据互信息判别单字如何结合。假设分词后为"aa b cc"，那么比较"aa b"与"b cc"的互信息，
        /// 比如"aa b"的互信息比较大，那么"aa b"有较大的成词概率
        /// </summary>
        /// <param name="word"></param>
        private static void Update_Mi(Word word)
        {
            var value = word.value;
            if (value.Length == 1)
            {
                word.mi = Math.Sqrt(word.freq / (double)_total_count);
            }
            else
            {
                double maxfreqmulti = 0;
                for (int i = 1; i < value.Length; ++i)
                {
                    var left = value.Substring(0, i);
                    var right = value.Substring(i);
                    double multifreq = _dict[left].freq * _dict[right].freq;
                    if (maxfreqmulti < multifreq)
                        maxfreqmulti = multifreq;
                }
                // p(x,y)/[p(x)p(y)] = freq(x,y) * T / [freq(x) * freq(y)]
                word.mi = _total_count * word.freq / maxfreqmulti;
            }
        }

        public class Word : IComparable<Word>
        {
            /// <summary>
            /// 左邻字-频率字典
            /// </summary>
            private IDictionary<char, int> _l_dict_char_freq;
            /// <summary>
            /// 右邻字-词频字典
            /// </summary>
            private IDictionary<char, int> _r_dict_char_freq;
            /// <summary>
            /// 所有左邻字总数量，等于<seealso cref="_l_dict_char_freq.Sum(p => p.Value)"/>
            /// </summary>
            private int _l_char_freq_total;
            /// <summary>
            /// 所有右邻字总数量，等于<seealso cref="_r_dict_char_freq.Sum(p => p.Value)"/>
            /// </summary>
            private int _r_char_freq_total;
            /// <summary>
            /// 当前词频
            /// </summary>
            public int freq;
            /// <summary>
            /// 当前词出现概率
            /// </summary>
            public double prob;
            /// <summary>
            /// 当前词互信息
            /// 互信息表示词内部凝固程度，若不考虑互信息，可能会找出"的电影"、"了一"等错误词语
            /// </summary>
            public double mi;
            /// <summary>
            /// 当前词的熵，等于<seealso cref="r_ent"/>与<seealso cref="l_ent"/>的较小值
            /// 熵表示词本身的自由程度，若不考虑熵，则可能会找出半个词
            /// </summary>
            public double entropy;
            /// <summary>
            /// 右邻字熵
            /// </summary>
            public double r_ent;
            /// <summary>
            /// 左邻字熵
            /// </summary>
            public double l_ent;
            /// <summary>
            /// 当前词的字符串值
            /// </summary>
            public string value;

            public Word(string val, int f)
            {
                value = val;
                freq = f;
                _l_dict_char_freq = new Dictionary<char, int>(2000);
                _r_dict_char_freq = new Dictionary<char, int>(2000);
            }

            public void Add_Char(char c, bool isleft)
            {
                if(isleft)
                {
                    if (_l_dict_char_freq.TryGetValue(c, out int f))
                        _l_dict_char_freq[c] = f + 1;
                    else
                        _l_dict_char_freq[c] = 1;
                    ++_l_char_freq_total;
                }
                else
                {
                    if (_r_dict_char_freq.TryGetValue(c, out int f))
                        _r_dict_char_freq[c] = f + 1;
                    else
                        _r_dict_char_freq[c] = 1;
                    ++_r_char_freq_total;
                }
            }

            public int CompareTo(Word other) => this.freq.CompareTo(other.freq);

            public override string ToString() => $"{value}, freq: {freq}, ent: {entropy}, mi: {mi}";

            

            /// <summary>
            /// 更新熵。取左熵与右熵的较小值，以左邻字熵为例，
            /// E(left) = - \sum_{left_char} p(left_char) * log[p(left_char)]
            /// 分词后再根据熵判别单字如何结合。假设分词后为"aa b cc"，那么比较"aa"的右邻字熵与"cc"的左邻字熵，
            /// 如果"aa"右邻字熵比较大，那么"aa"独立成词的概率比较大，于是将"b"与"cc"结合
            /// </summary>
            /// <param name="total_count"></param>
            public void Update_Entropy(int total_count)
            {
                prob = freq / total_count;
                double left_entropy = 0.0;
                double right_entropy = 0.0;
                foreach(var p in _l_dict_char_freq)
                {
                    var pr = p.Value / (double)_l_char_freq_total;
                    left_entropy -= pr * Math.Log(pr);
                }
                l_ent = left_entropy;
                foreach(var p in _r_dict_char_freq)
                {
                    var pr = p.Value / (double)_r_char_freq_total;
                    right_entropy -= pr * Math.Log(pr);
                }
                r_ent = right_entropy;
                entropy = left_entropy > right_entropy ? right_entropy : left_entropy;
            }
        }
    }
}
