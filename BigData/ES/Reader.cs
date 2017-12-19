using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Elasticsearch.Net;

namespace BigData.ES
{
    public class Reader<T> where T : class
    {
        #region fields & .ctor
        private Config _config;
        private ElasticClient _client;

        public Reader(Config config)
        {
            _config = config;
            _client = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(_config.ES_CONN_STR.Split(',').Select(u => new Uri(u)))));
        }
        #endregion

        #region Query
        public ISearchResponse<T> Query(QueryDes qd)
        {
            var s = new SearchDescriptor<T>().Index(_config.ES_INDEX).Type(_config.ES_TYPE).From(qd.From).Take(qd.Take)
                .Source(sr => GetSource(sr, qd))
                .Query(q => GetQuery(q, qd));
            if(qd.SortField != null)
            {
                if (qd.IsAsc)
                    s.Sort(st => st.Ascending(qd.SortField));
                else
                    s.Sort(st => st.Descending(qd.SortField));
            }

            if (qd.TerminateAfter > 0)                  // 设置单个节点上的数据查询截断阈值，提高响应速度（但会导致数据搜索不全面）
                s.TerminateAfter(qd.TerminateAfter);

            if(qd.Aggs != null)
                s.Aggregations(agg => Get_Agg(agg, qd));

            if (qd.HLPreTag != null)
                s.Highlight(hl => Get_HL(hl, qd));

            return _client.Search<T>(s);
        }

        public ISourceFilter GetSource(SourceFilterDescriptor<T> sr, QueryDes qd)
        {
            if (qd.Srcs == null || qd.Srcs.Count == 0)
                return qd.IsInclude ? sr.ExcludeAll() : sr.IncludeAll();

            var arr = qd.Srcs.Select(fld => fld.ToString()).ToArray();
            return qd.IsInclude ? sr.Includes(f => f.Fields(arr)) : sr.Excludes(f => f.Fields(arr));
        }

        /// <summary>
        /// 获取聚合
        /// </summary>
        /// <param name="agg"></param>
        /// <param name="ci"></param>
        /// <returns></returns>
        private IAggregationContainer Get_Agg(AggregationContainerDescriptor<T> agg, QueryDes qd)
        {
            var ls = qd.Aggs;
            if (ls == null) return agg;

            foreach (var s in ls)
            {
                switch (s)
                {
                    case 1:
                        agg.Terms("m_area", t => t.Field("m_area").Size(33));
                        break;
                    case 2:
                        agg.Range("od_regm", r => r.Field("od_regm").Ranges(rs => rs.To(100), rs => rs.From(100).To(500), rs => rs.From(500).To(1000),
                                                                rs => rs.From(1000)));
                        break;
                    case 3:
                        agg.DateHistogram("od_regdate", t => t.Field("od_regdate").Interval(DateInterval.Year)
                                            .MinimumDocumentCount(1).ExtendedBounds(new DateTime(1900, 1, 1), DateTime.Now));
                        break;
                }
            }
            return agg;
        }

        private HighlightDescriptor<T> Get_HL(HighlightDescriptor<T> hl, QueryDes qd) => hl
            .PreTags(qd.HLPreTag)
            .PostTags(qd.HLPostTag)
            .Fields(
                f => f.Field("oc_code"),
                f => f.Field("oc_number")
            );

        private QueryContainer GetQuery(QueryContainerDescriptor<T> q, QueryDes qd)
        {
            var input = qd.MultiFilters[10].First();
            var score_idx = input.Length + 1;
            return q.FunctionScore(fs => fs
                .BoostMode(FunctionBoostMode.Sum)
                .ScoreMode(FunctionScoreMode.Sum)
                .Functions(fun => fun
                    .ScriptScore(ss => ss
                        .Script(s => s
                            .Inline("for(s in doc['scores']) {if(s.contains(\""                 // scores: ["xxx-10","yyy-20",...]
                                    + input 
                                    + "\")){ return 1000 * Double.parseDouble(s.substring(" 
                                    + score_idx 
                                    + ")) + _score + doc['oc_weight'].value ;}} return _score + doc['oc_weight'].value;")
                            .Lang("painless")))
                    .FieldValueFactor(f => f
                        .Filter(flt => flt.Term(t => t.Field("<field name>").Value("<field value>")))
                        .Factor(1)
                        .Field("<field name whose value type is number>")
                        .Missing(1)
                    )
                )
                .Query(qq => qq.Bool(b => b
                    .Filter(f => GetMultiFilterContainer(f, qd))
                    .Must(m => GetQueryContainer(m, qd))
                    )
                )
            );
        }

        private QueryContainer GetMultiFilterContainer(QueryContainerDescriptor<T> q, QueryDes qd)
        {
            if (qd.MultiFilters == null) return q;

            var qcs = new List<QueryContainer>(qd.MultiFilters.Count);
            foreach(var p in qd.MultiFilters)
            {
                var qc = new QueryContainer();
                switch(p.Key)
                {
                    case 0:
                        foreach (var s in p.Value)
                            qc |= q.Term(t => t.Field("<field name(id 0) whose value is not tokenized>").Value(s));
                        break;
                    case 1:
                        foreach (var s in p.Value)
                            qc |= q.Prefix(pre => pre.Field("<field name(id 1) whose value is standard tokenized").Value(s));
                        break;
                    case 2:
                        foreach(var s in p.Value)
                        {
                            var year = int.Parse(s);
                            qc |= q.DateRange(d => d.Field("<field name(id 2) whose value has a datetime type").GreaterThanOrEquals(new DateTime(year)).LessThan(new DateTime(year + 1)));
                            //qc |= q.DateRange(d => d.Field("<field name(id 2) whose value has a datetime format").GreaterThanOrEquals(s).LessThan((year+1).ToString()).Format("yyyy"));
                        }
                        break;
                    case 3:
                        foreach(var s in p.Value)
                        {
                            var segs = s.Split('-');
                            if (s[0] == '*')
                                qc |= q.Range(r => r.Field("<field name(id 3) whose value has a number type>").LessThan(double.Parse(segs[1])));
                            else if(s[s.Length - 1] == '*')
                                qc |= q.Range(r => r.Field("<field name(id 3) whose value has a number type>").GreaterThanOrEquals(double.Parse(segs[0])));
                            else
                                qc |= q.Range(r => r.Field("<field name(id 3) whose value has a number type>").GreaterThanOrEquals(double.Parse(segs[0])).LessThan(double.Parse(segs[1])));
                        }
                        break;
                    case 4:
                        var f = p.Value.FirstOrDefault();
                        if (f[0] == 't')
                            qc = q.Exists(e => e.Field("<field name(id 4)>"));
                        else if (f[0] == 'f')
                            qc = q.Bool(b => b.MustNot(m => m.Exists(e => e.Field("<field name(id 4)>"))));
                        break;
                    case 5:
                        foreach (var s in p.Value)
                        {
                            if (s[0] == 'm')
                                qc |= q.Regexp(r => r.Field("<field name(id 5)>").Value(@"\+?(00)?((86)|(86-))?1[345689]\d{9}"));   // mobile phone
                            else if (s[0] == 't')
                                qc |= q.Regexp(r => r.Field("<field name(id 5)>").Value(@"\+?(00)?((86)|(86-))?((0\d{3}[\s/-])|(\(0\d{3})))?\d{7,8}"));     // telephone
                        }
                        break;
                }
                qcs.Add(qc);
            }
            var res = qcs[0];
            for (int i = 1; i < qcs.Count; i++)
                res &= qcs[i];
            return res;
        }

        private QueryContainer GetQueryContainer(QueryContainerDescriptor<T> q, QueryDes qd)
        {
            var qcs = new List<QueryContainer>();
            foreach(var p in qd.MultiFilters)
            {
                var qc = new QueryContainer();
                switch(p.Key)
                {
                    case 10:
                        foreach(var s in p.Value)
                        {
                            qc |= GetQueryContainer_Generic(q, "<field name(id 10)>", s);
                        }
                        break;
                    
                }
                qcs.Add(qc);
            }
            var res = qcs[0];
            for (int i = 1; i < qcs.Count; i++)
                res &= qcs[i];
            return res;
        }

        private QueryContainer GetQueryContainer_Generic(QueryContainerDescriptor<T> q, string name, string value)
        {
            var q1 = q.MatchPhrase(m => m.Field(name).Query(value).Boost(10)) | q.Match(m => m.Field(name).Query(value).MinimumShouldMatch(MinimumShouldMatch.Percentage(80)));
            var q2 = q.MatchPhrase(m => m.Field(name).Query(value.Substring(0, 2)));
            return q.DisMax(dm => dm.Queries(q1 & q2, q1));
        }



        #endregion

        #region Aggregation
        /// <summary>
        /// 统计数据最大值
        /// </summary>
        /// <param name="obj">目标字段名</param>
        /// <param name="dim">按指定维度统计，维度字段名</param>
        /// <returns></returns>
        /// <remarks>obj的值类型为number</remarks>
        public ISearchResponse<T> Statistic_Max(string obj, string dim)
        {
            /*          d1  d2  d3  ...     ->  statistic
             *      o1  *   *   *   ...     ->  max(line1)
             *      o2  *   *   *   ...     ->  max(line2)
             *      o3  *   *   *   ...     ->  max(line3)
             *      ...     ...     ...     ->  ...
             * */
            var response = _client.Search<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Take(0).Query(q => q/*if there is some filter conditions, fill it here*/)
                .Aggregations(agg => agg
                    .Terms(dim/*自定义名称，这里取维度字段名*/, t => t
                        .Field(dim)
                        .Size(32/*统计指定维度上最多32个数据点*/)
                        .Aggregations(a => a.Max(obj/*自定义名称，这里取被统计字段名*/, m => m.Field(obj))))   //! 统计 obj 的最大值，显然 obj 的值类型为 number
                )
            );
            return response;
        }

        /// <summary>
        /// 统计指定维度上指定字段的函数值的总和的最大值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public ISearchResponse<T> Statistic_Sum(string obj, string dim)
        {
            /*          d1          d2        ...     ->  sum               max
             *      o1  f(o1,d1)   f(o1,d2)   ...     ->  sum(f)    ----\
             *      o2  f(o2,d1)   f(o2,d2)   ...     ->  sum(f)    ------  max(sum)
             *      ...     ...     ...       ...     ->  ...       ----/
             * */
            var response = _client.Search<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Take(0).Query(q => q/*if there is some filter conditions, fill it here*/)
                .Aggregations(agg => agg
                    .Terms(dim/*自定义名称，这里取维度字段名，名称关系到下面的桶路径*/, t => t
                        .Field(dim)
                        .Size(32/*统计指定维度上最多32个数据点*/)
                        .Aggregations(a => a
                            .Sum(obj/*自定义名称，这里取被统计字段名，名称关系到下面的桶路径*/, sm => sm
                                .Script(scr => scr.Inline($"doc[{obj}].length"/*y=list.length*/).Lang("expression"))   // 这里统计的是一个列表字段，统计其数量总和
                            )
                        )
                    )
                    .MaxBucket($"sum_{obj}{dim}"/*自定义名称*/, mb => mb.BucketsPath($"{dim}>{obj}"/*桶路径，规则为从外层Aggs名称到内层Aggs名称，使用'>'分隔*/))     // 选取最大桶，表示只关心数量总和最大的数据
                )
            );
            return response;
        }

        /// <summary>
        /// 统计目标字段值的平均值
        /// </summary>
        /// <param name="obj">目标字段名</param>
        /// <returns></returns>
        public ISearchResponse<T> Statistic_Ave(string obj)
        {
            var response = _client.Search<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Take(0).Query(q => q/*if there is some filter conditions, fill it here*/)
                .Aggregations(agg => agg
                    .Average("ave"/*自定义名称，名称关系到下面的桶路径*/, ave => ave
                        .Script(scr => scr.Inline($"doc[{obj}]").Lang("expression"))
                    )
                    .AverageBucket(obj, ab => ab.BucketsPath("ave"/*桶路径，规则为从外层Aggs名称到内层Aggs名称，使用'>'分隔*/))
                )
            );
            return response;
        }

        /// <summary>
        /// 按时间统计
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dim"></param>
        /// <param name="di">时间间隔</param>
        /// <returns></returns>
        public ISearchResponse<T> Statistic_Ave_Time(string obj, string dim, DateInterval di)
        {
            var response = _client.Search<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Take(0).Query(q => q/*if there is some filter conditions, fill it here*/)
                .Aggregations(agg => agg
                    .DateHistogram("dh"/*自定义名称，名称关系到下面的桶路径*/, dh => dh                                                  // 按时间统计
                        .Field(dim)
                        .Interval(di)
                        .Aggregations(a => a
                            .Average("ave"/*自定义名称，名称关系到下面的桶路径*/, ave => ave                                             // 统计指定字段的表达式的值的平均
                                .Script(scr => scr.Inline($"doc[{dim}]").Lang("expression"))
                            )
                        )
                    )
                    .AverageBucket("ave_dhave", ab => ab.BucketsPath("dh>ave"/*桶路径，规则为从外层Aggs名称到内层Aggs名称，使用'>'分隔*/))     // 统计平均所有的平均值
                )
            );
            return response;
        }

        /// <summary>
        /// 按时间统计
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dim"></param>
        /// <param name="di">时间间隔</param>
        /// <returns></returns>
        public ISearchResponse<T> Statistic_Ave_Area(string obj, string dim)
        {
            var response = _client.Search<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Take(0).Query(q => q/*if there is some filter conditions, fill it here*/)
                .Aggregations(agg => agg
                    .Terms(dim/*自定义名称，名称关系到下面的桶路径*/, t => t                                                  // 按时间统计
                        .Field(dim)
                        .Size(32/*dim维度上最多取32个值*/)
                        .MinimumDocumentCount(100/*低于100数量的不纳入统计*/)
                        .Aggregations(a => a
                            .Average(obj/*自定义名称，名称关系到下面的桶路径*/, ave => ave                                             // 统计指定字段的表达式的值的平均
                                .Script(scr => scr.Inline($"doc[{dim}]").Lang("expression"))
                            )
                        )
                    )
                    .AverageBucket($"ave_{dim}{obj}", ab => ab.BucketsPath($"{dim}>{obj}"/*桶路径，规则为从外层Aggs名称到内层Aggs名称，使用'>'分隔*/))     // 统计平均所有的平均值
                )
            );
            return response;
        }
        #endregion

        #region Induce
        public static Outcome<T> Induce(ISearchResponse<T> r)
        {
            var oc = new Outcome<T>();
            oc.docs = r.Hits.Select(h => new Doc<T>()
            {
                doc = h.Source,
                hl = h.Highlights.ToDictionary(hl => hl.Key, hl => hl.Value.Highlights.FirstOrDefault())
            }).ToList();
            if(r.Aggregations.Count > 0)
            {
                oc.aggss = ExtractComAggs(r);
            }
            return oc;
        }

        private static List<Aggs> ExtractComAggs(ISearchResponse<T> r)
        {
            var aggs = new List<Aggs>();
            foreach (var ag in r.Aggregations)
            {
                switch (ag.Key)
                {
                    case "m_area":
                        var areas = r.Aggs.Terms("m_area");
                        var areaAgg = new Aggs("m_area");
                        foreach (var b in areas.Buckets)
                        {
                            var aggItem = new Agg();
                            aggItem.count = b.DocCount ?? 0;
                            areaAgg.aggs.Add(aggItem);
                        }
                        aggs.Add(areaAgg);
                        break;
                    case "oc_status":
                        var statuss = r.Aggs.Terms("oc_status");
                        var statusAgg = new Aggs("oc_status");
                        foreach (var b in statuss.Buckets)
                        {
                            var aggItem = new Agg();
                            aggItem.count = b.DocCount ?? 0;
                            statusAgg.aggs.Add(aggItem);
                        }
                        aggs.Add(statusAgg);
                        break;
                    case "gb_cat":
                        var trades = r.Aggs.Terms("gb_cat");
                        var tradeAgg = new Aggs("gb_cat");
                        foreach (var b in trades.Buckets)
                        {
                            var aggItem = new Agg();
                            aggItem.count = b.DocCount ?? 0;
                            tradeAgg.aggs.Add(aggItem);
                        }
                        aggs.Add(tradeAgg);
                        break;
                    case "od_regdate":
                        var dates = r.Aggs.DateHistogram("od_regdate");
                        var dateAgg = new Aggs("od_regdate");
                        foreach (var b in dates.Buckets)
                        {
                            var aggItem = new Agg();
                            dateAgg.aggs.Add(aggItem);
                        }
                        aggs.Add(dateAgg);
                        break;
                    case "od_regm":
                        var regms = r.Aggs.DateRange("od_regm");
                        var regmAgg = new Aggs("od_regm");
                        foreach (var b in regms.Buckets)
                        {
                            var aggItem = new Agg();
                            
                            aggItem.count = b.DocCount;
                            regmAgg.aggs.Add(aggItem);
                        }
                        aggs.Add(regmAgg);
                        break;
                }
            }
            return aggs;
        }

        private static List<Aggs> ExtractPipeAggs(ISearchResponse<T> r)
        {
            var aggss = new List<Aggs>();
            foreach (var ag in r.Aggregations)
            {
                var segs = ag.Key.Split('_');
                switch (segs[0])
                {
                    case "area":
                        var areas = r.Aggs.Terms("area");
                        var areaAggs = new Aggs("area");
                        foreach (var b in areas.Buckets)
                        {
                            if (b.Aggregations.Count > 0)
                            {
                                var agg = new Agg();
                                agg.count = b.DocCount ?? 0;
                                agg.value = Math.Round(b.Average(segs[1]/*"ave_sal"*/).Value ?? 0, 2);
                                areaAggs.aggs.Add(agg);
                            }
                        }
                        aggss.Add(areaAggs);
                        break;
                    case "mon":
                        var mons = r.Aggs.DateHistogram("mon");
                        var monAggs = new Aggs("mon");
                        foreach (var b in mons.Buckets)
                        {
                            if (b.Aggregations.Count > 0)
                            {
                                var agg = new Agg();
                                agg.count = b.DocCount ?? 0;
                                agg.value = Math.Round(b.Average(/*"ave_sal"*/segs[1]).Value ?? 0, 2);
                                monAggs.aggs.Add(agg);
                            }
                        }
                        aggss.Add(monAggs);
                        break;
                }
            }
            return aggss;
        }
        #endregion
    }
}
