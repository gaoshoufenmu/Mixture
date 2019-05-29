using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace StatisticCS
{
    /// <summary>
    /// 属性（特征）
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// 当前属性在属性列表（特征向量）中的索引位置
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 属性的取值空间
        /// </summary>
        public List<string> Values { get; set; }

        public Attribute(int index, string name, List<string> values)
        {
            Index = index;
            Name = name;
            Values = values;
        }
    }

    /// <summary>
    /// 数据
    /// </summary>
    public class Data
    {
        public static Regex attrRegex = new Regex(@"^@ATTRIBUTE\s+(?<name>.*?)\s+{(?<values>(.+))}$", RegexOptions.Compiled);
        /// <summary>
        /// 数据点集合
        /// </summary>
        public List<Dictionary<string, string>> Examples { get; set; }
        /// <summary>
        /// 属性字典，最后一个为目标属性
        /// key为属性名，value为属性的可取值
        /// </summary>
        public Dictionary<string, string[]> Attributes { get; set; }
        /// <summary>
        /// 目标属性名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 给定数据文件地址，创建数据对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Data Create(string path)
        {
            var attrs = new List<Tuple<string, string[]>>();
            string target = null;
            var lines = File.ReadAllLines(path);
            var examples = new List<Dictionary<string, string>>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("@ATTRIBUTE"))
                {
                    var match = attrRegex.Match(line);
                    var name = match.Groups["name"].Value;
                    var values = match.Groups["values"].Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    attrs.Add(new Tuple<string, string[]>(name, values));
                    target = name;
                }
                else if (line[0] != '@')
                {
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var example = new Dictionary<string, string>();
                    for (int i = 0; i < segs.Length; i++)
                    {
                        var name = attrs[i].Item1;
                        var value = segs[i];
                        example.Add(name, value);
                    }
                    examples.Add(example);
                }
            }
            var data = new Data() { Examples = examples, Attributes = attrs.ToDictionary(t => t.Item1, t => t.Item2), Target = target };
            return data;
        }
    }

    public class Node1
    {
        /// <summary>
        /// 特征的取值
        /// </summary>
        public List<string> features;
        /// <summary>
        /// 特征类型，连续 or 离散
        /// </summary>
        public string feature_type;
        /// <summary>
        /// 特征名
        /// </summary>
        public string splitfeature;
        /// <summary>
        /// 当前节点覆盖的样本中，各个类别的数量统计
        /// </summary>
        public double[] classCount;
        /// <summary>
        /// 当前节点覆盖的样本数
        /// </summary>
        public int count;
        /// <summary>
        /// 子节点列表
        /// </summary>
        public List<Node1> childNodes;
        /// <summary>
        /// 父节点
        /// </summary>
        public Node1 parent;
        /// <summary>
        /// 当前节点覆盖的样本中，占比最大的类
        /// </summary>
        public string maxClass;
        /// <summary>
        /// 树的深度
        /// </summary>
        public int deep;

        /// <summary>
        /// 节点占比最大的类在<seealso cref="classCount"/>中的索引，用于剪枝
        /// </summary>
        public int result;
        /// <summary>
        /// 叶节点数量
        /// </summary>
        public int leafCount;
        /// <summary>
        /// 叶节点分类错误总数
        /// </summary>
        public int leafError;

        /// <summary>
        /// 获取剪枝为叶节点后的错误数
        /// </summary>
        /// <returns></returns>
        public double GetErrorCount()
        {
            return count - classCount[result];
        }
        /// <summary>
        /// 设置节点覆盖的样本中各个分类的数量
        /// </summary>
        /// <param name="count"></param>
        public void SetClassCount(double[] count)
        {
            this.classCount = count;
            double max = classCount[0];
            int idx = 0;

            for(int i = 0; i < count.Length; i++)
            {
                if(max < count[i])
                {
                    max = count[i];
                    idx = i;
                }
            }
            this.result = idx;
        }
    }
    /// <summary>
    /// 分裂信息类
    /// </summary>
    public class SplitInfo1
    {
        /// <summary>
        /// 分裂的属性索引
        /// </summary>
        public int splitIndex;
        /// <summary>
        /// 数据类型
        /// </summary>
        public int type;
        /// <summary>
        /// 特征的取值
        /// </summary>
        public List<string> features;
        public List<int>[] temp;
        public double[][] classCount;
    }

    public class Util1
    {
        /// <summary>
        /// 树最大深度，达到此值后停止分裂
        /// </summary>
        public const int maxDeep = 10;
        /// <summary>
        /// 最小样本数，小于此值后停止分裂
        /// </summary>
        public const int LimitCount = 100;
        /// <summary>
        /// 输入特征的值类型，0: 离散， 1: 连续
        /// </summary>
        public static int[] types = new[] { 1, 0 };

        /// <summary>
        /// 计算某一结点的信息熵
        /// 各个叶节点的信息熵的和
        /// </summary>
        /// <param name="counts">每个分类数量</param>
        /// <param name="countAll">样本总数</param>
        /// <returns></returns>
        public static double CalEntropy(double[] counts, int countAll)
        {
            double sum = 0;
            for(int i = 0; i < counts.Length; i++)
            {
                if (counts[i] == 0) continue;

                double rate = counts[i] / countAll;
                sum += rate * Math.Log(rate, 2);
            }
            return sum;
        }
        /// <summary>
        /// 是否停止分裂
        /// </summary>
        /// <param name="node"></param>
        /// <param name="entropy"></param>
        /// <param name="isUsed"></param>
        /// <returns></returns>
        public static bool IfStop(Node1 node, double entropy, int[] isUsed)
        {
            var counts = node.classCount;
            var countAll = node.count;
            int maxIndex = 0;               // 占比最大的分类值在 classCount 数组中的索引

            int deep = node.deep;

            // 最后一项表示分类，而非输入特征属性
            bool flag = true;       // 如果输入属性已经用完
            for (int i = 0; i < isUsed.Length - 1; i++)
            {
                if (isUsed[i] == 0)
                {
                    flag = false;       // 输入属性尚未用完
                    break;
                }
            }

            if (deep >= maxDeep || entropy == 0 || countAll < LimitCount || flag)
            {
                maxIndex = node.result + 1;
                node.feature_type = "result";
                node.features = new List<string>() { maxIndex + "" };
                node.leafError = countAll - (int)counts[node.result];
                node.leafCount = 1;
                return true;
            }
            return false;
        }

        public static Node1 FindBestSplit(Node1 node, List<int> nums, int[] isUsed)
        {
            // 计算node子树系统的信息熵
            double entropy = CalEntropy(node.classCount, node.count);
            if(IfStop(node, entropy, isUsed))
            {
                return node;    // 停止分裂，对node做适当的赋值以标记分裂停止，返回这个节点
            }

            var info = new SplitInfo1();
            var countAll = node.count;

            for(int i = 0; i < isUsed.Length - 1; i++)
            {
                if (isUsed[i] == 1) continue;

                if(types[i] == 0)       // 离散型属性值
                {

                }
            }
            return null;
        }

    }
}
