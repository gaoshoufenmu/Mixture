using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigData.ES
{
    /// <summary>
    /// ES查询输出Wrapper类
    /// </summary>
    /// <typeparam name="T">ES某一类型对应的model类</typeparam>
    public class Outcome<T>
    {
        /// <summary>
        /// 花费时间，unit: ms
        /// </summary>
        public float took { get; private set; }
        /// <summary>
        /// 所有查询匹配文档数
        /// </summary>
        public long total { get; private set; }

        /// <summary>
        /// 文档列表
        /// </summary>
        public List<Doc<T>> docs { get; set; }

        /// <summary>
        /// 聚合统计
        /// </summary>
        public List<Aggs> aggss { get; set; }

    }

    /// <summary>
    /// 文档Wrapper类
    /// </summary>
    /// <typeparam name="T">ES某一类型对应的model类</typeparam>
    public class Doc<T>
    {
        /// <summary>
        /// 文档类
        /// </summary>
        public T doc { get; set; }
        /// <summary>
        /// 高亮，key为字段名，value为含有高亮html tag的文本
        /// </summary>
        public Dictionary<string, string> hl { get; set; }
    }

    /// <summary>
    /// 聚合统计类
    /// </summary>
    public class Aggs
    {
        /// <summary>
        /// 聚合名称
        /// </summary>
        public string name;
        /// <summary>
        /// 聚合项列表
        /// </summary>
        public List<Agg> aggs;

        public Aggs(string name)
        {
            this.name = name;
            aggs = new List<Agg>();
        }
    }

    /// <summary>
    /// 聚合项
    /// </summary>
    public class Agg
    {
        /// <summary>
        /// 项名称
        /// </summary>
        public string label;
        /// <summary>
        /// 项指标数量
        /// </summary>
        public long count;
        /// <summary>
        /// 项值
        /// </summary>
        public double value;
    }
}
