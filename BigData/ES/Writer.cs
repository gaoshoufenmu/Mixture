using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Nest;
using Elasticsearch.Net;

namespace BigData.ES
{
    /// <summary>
    /// 向ES写数据
    /// </summary>
    /// <typeparam name="T">ES Type实体类型</typeparam>
    public class Writer<T> where T : class
    {
        private Config _config;
        private ElasticClient _client;

        public Writer(Config config)
        {
            _config = config;
            _client = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(_config.ES_CONN_STR.Split(',').Select(u => new Uri(u)))));
        }

        public void CreateIndex()
        {
            if (!_client.IndexExists(_config.ES_INDEX).Exists)
                _client.CreateIndex(_config.ES_INDEX, idx => idx
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .Analysis(ana => ana
                            .Analyzers(al => al
                                .Standard("sstd", std => std.MaxTokenLength(1))          // 严格单字符切分
                                .Pattern("sep", sep => sep.Pattern(@"[-\.\|,\s]"))      // 指定字符切分
                                .Pattern("ascii", asci => asci.Pattern(@"\p{ASCII}"))   // 任意ascii字符切分
                                )
                            )
                        )
                    );
        }

        public void CreateMap()
        {
            if (!_client.TypeExists(_config.ES_INDEX, _config.ES_TYPE).Exists)
                _client.Map<T>(m => m.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Properties(WalkOver()));
        }

        public IBulkResponse BulkIndex(IEnumerable<T> ts) => _client.Bulk(b => b.Index(_config.ES_INDEX).Type(_config.ES_TYPE).IndexMany(ts));

        #region Update
        /// <summary>
        /// 批量更新单个字段
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public IBulkResponse BulkUpdate(IEnumerable<Tuple<string, string>> ts, string field) =>
            _client.Bulk(b => b
                .Index(_config.ES_INDEX)
                .Type(_config.ES_TYPE)
                .UpdateMany<Tuple<string, string>, object>(ts, (u, t) => u
                    .Id(t.Item1)
                    .Doc(Foundation.IL.Proxy.CreateAnonymous(new[] { field}, new [] { typeof(string) }, new[] { t.Item2} ))));

        /// <summary>
        /// 使用脚本批量更新单个列表字段
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public IBulkResponse BulkUpdate_script(IEnumerable<Tuple<string, string>> ts, string field) =>
            _client.Bulk(b => b
                .Index(_config.ES_INDEX)
                .Type(_config.ES_TYPE)
                .UpdateMany<Tuple<string, string>, object>(ts, (u, t) => u
                    .Id(t.Item1)
                    .Script(s => s.Inline($@"
if(!ctx._source.containsKey('{field}'))
    ctx._source.{field} = new ArrayList();
if(!ctx._source.{field}.contains(params.n))
    ctx._source.{field}.add(params.n); 
")
                    .Lang("painless").Params(new Dictionary<string, object>() { ["n"] = t.Item2 })
                    )
                )
            );

        public IBulkResponse BulkUpdate_script(IEnumerable<Tuple<string, List<string>>> ts, string field) =>
            _client.Bulk(b => b
                .Index(_config.ES_INDEX)
                .Type(_config.ES_TYPE)
                .UpdateMany<Tuple<string, List<string>>, object>(ts, (u, t) => u
                    .Id(t.Item1)
                    .Script(s => s.Inline($@"
if(!ctx._source.containsKey('{field}'))
    ctx._source.{field} = new ArrayList();
for(int i = 0; i < params.n.length; i++)
    if(!ctx._source.{field}.contains(params.n.get(i)))
        ctx._source.{field}.add(params.n.get(i));
")
                    .Lang("painless").Params(new Dictionary<string, object>() { ["n"] = t.Item2 })
                    )
                )
            );

        #endregion

        #region Delete
        public void DeleteByQuery(string field) =>
            _client.DeleteByQuery<T>(s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE).Query(q => q.Bool(b => b.MustNot(mn => mn.Exists(e => e.Field(field))))));

        public void DeleteById(string id) =>
            _client.Delete<T>(id, s => s.Index(_config.ES_INDEX).Type(_config.ES_TYPE));
        #endregion

        private Func<PropertiesDescriptor<T>, IPromise<IProperties>> WalkOver()
        {
            var t = typeof(T);
            var propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return p =>
            {
                for (int i = 0; i < propInfos.Length; i++)
                {
                    var p_name = propInfos[i].Name;
                    var p_type = propInfos[i].PropertyType;
                    var p_attr = (MapAttr)p_type.GetCustomAttribute(typeof(MapAttr));

                    switch (p_attr.type)
                    {
                        case FieldType._bool_:
                            p.Boolean(b => b.Name(p_name));
                            break;
                        case FieldType._byte_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Byte));
                            break;
                        case FieldType._double_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Double));
                            break;
                        case FieldType._float_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Float));
                            break;
                        case FieldType._int_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Integer));
                            break;
                        case FieldType._long_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Long));
                            break;
                        case FieldType._short_:
                            p.Number(n => n.Name(p_name).Type(NumberType.Short));
                            break;
                        case FieldType._date_:
                            p.Date(n => n.Name(p_name));
                            break;
                        case FieldType._keyword_:
                            p.Keyword(k => k.Name(p_name));
                            break;
                        case FieldType._geo_:
                            p.GeoPoint(g => g.Name(p_name));
                            break;
                        case FieldType._text_:
                            p.Text(txt => txt.Name(p_name).Analyzer(p_attr.tokenizer));
                            break;
                    }
                }
                return p;
            };
        }
    }
}
