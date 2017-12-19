using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigData.ES
{
    /// <summary>
    /// Query description
    /// </summary>
    public class QueryDes
    {
        public string HLPreTag { get; set; }
        public string HLPostTag => GetHLPostTag();

        public int TerminateAfter { get; set; }
        public int From { get; set; }
        public int Take { get; set; }
        /// <summary>
        /// 每个字段多选
        /// key: 字段在类型中的唯一id，value: 字段的多个筛选值，"or" 的关系
        /// </summary>
        public Dictionary<int, HashSet<string>> MultiFilters { get; set; }

        public List<int> Aggs { get; set; }
        public bool IsAsc { get; set; }
        public string SortField { get; set; }
        public ISet<string> Srcs { get; set; }
        public bool IsInclude { get; set; }


        public void AddMultiFilter(int i, string f)
        {
            if (MultiFilters == null)
                MultiFilters = new Dictionary<int, HashSet<string>>();
            if (!MultiFilters.TryGetValue(i, out var set))
            {
                set = new HashSet<string>();
                set.Add(f);
                MultiFilters[i] = set;
            }
            else
                set.Add(f);
        }

        public void AddMultiFilter(int i, List<string> fs)
        {
            if (MultiFilters == null)
                MultiFilters = new Dictionary<int, HashSet<string>>();
            HashSet<string> set;
            if (!MultiFilters.TryGetValue(i, out set))
            {
                set = new HashSet<string>(fs);
                MultiFilters[i] = set;
            }
            else
            {
                foreach (var f in fs)
                    set.Add(f);
            }
        }

        private string GetHLPostTag()
        {
            var sb = new StringBuilder(7);
            foreach(var c in HLPreTag)
            {
                if (c == '<')
                    sb.Append("</");
                else if (c == ' ')
                {
                    sb.Append(">");
                    break;
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
