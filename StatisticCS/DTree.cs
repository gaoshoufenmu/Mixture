using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticCS
{
    public class DTree
    {
        private Node _root = new Node();
        /// <summary>
        /// 所有的分类值
        /// </summary>
        private string[] _classes;
        /// <summary>
        /// 决策树根结点
        /// </summary>
        public Node Root { get { return _root; } }
        private int _maxDeep;
        /// <summary>
        /// 最大深度，根节点深度为0
        /// </summary>
        public int MaxDeep { get { return _maxDeep; } }
        /// <summary>
        /// 给定训练数据集创建决策树
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DTree Create(Data data)
        {
            var dtree = new DTree();
            dtree._classes = data.Attributes[data.Target];
            var target = data.Target;
            var points = data.Examples;
            Create(data.Examples, data.Attributes.Keys.ToList(), data.Target, dtree._root, data.Attributes);
            return dtree;
        }


        private static void Create(List<Dictionary<string, string>> points, List<string> attrNames, string target, Node node, Dictionary<string, string[]> attrs)
        {
            if(attrNames.Count == 1)    // 没有属性（或者只剩一个目标target）
            {
                node.Class = Util.GetDefaultClass(points, target);  // 使用当前数据集中数量最多的分类
            }
            else
            {
                var bestAttr = Util.GetBestAttr(points, attrNames, target);     // 最佳属性
                var targetVals = points.Select(p => p[target]);                 // 当前数据集中分类值枚举
                node.Attr = bestAttr;
                if (Util.IsSameElem(targetVals))
                {
                    // 剩余的数据点分类值相同，node为叶节点
                    node.Class = targetVals.First();
                }
                else
                {
                    node.Children = new Dictionary<string, Node>();
                    // 根据最佳属性划分得到子空间枚举，遍历
                    foreach (var v in attrs[bestAttr])
                    {
                        var list = Util.GetSubRegion(points, bestAttr, v);
                        if (list.Count == 0)      
                        {
                            // 子空间无数据点，则使用父空间的默认分类
                            var newNode = new Node();
                            newNode.Class = Util.GetDefaultClass(points, target);
                            node.Children.Add(v, newNode);
                        }
                        else
                        {
                            // 子空间有数据点
                            var newAttrNames = attrNames.Where(n => n != bestAttr).ToList();
                            var newNode = new Node();
                            node.Children.Add(v, newNode);
                            Create(list, newAttrNames, target, newNode, attrs);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 给定数据点判别分类
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public string Judge(Dictionary<string, string> point) => Judge(_root, point);

        private string Judge(Node node, Dictionary<string, string> point)
        {
            if(node.Attr == null)
            {
                return node.Class;
            }
            else
            {
                var attrVal = point[node.Attr];
                return Judge(node.Children[attrVal], point);
            }
        }
        /// <summary>
        /// 剪枝
        /// </summary>
        public void Pruning()
        {
            var tuple = GetPrecNodes(_maxDeep);
            var leaves = GetInitLeaves();
            var deep = _maxDeep;    // 递归深度
            var unPrunedCount = 0;  // 某轮未被剪枝的数量
            while(deep > 0)
            {
                var nodes = GetPrecNodes(deep);
                foreach (var node in nodes)
                {
                    // 考察内部节点
                    if (node.Children != null && node.Children.Count > 0)
                    {
                        // 判断是否需要剪枝
                        var preLoss = GetLoss(leaves);
                        var fakeLeaves = GetPrunedLeaves(leaves, node);
                        var postLoss = GetLoss(fakeLeaves);
                        if (postLoss < preLoss)
                        {
                            // 需要剪枝，则进行剪枝
                            node.parent.Children[node.attrVal] = fakeLeaves[fakeLeaves.Count - 1];
                            leaves = fakeLeaves;    // 更新叶节点
                        }
                        else
                        {
                            unPrunedCount++;
                        }
                    }
                }
                if(deep == _maxDeep)    // 当前深度与最大深度保持同步，则需要检查是否需要修改最大深度
                {
                    if(unPrunedCount == 0)  // 本轮被考察节点全部被剪枝，则修改最大深度
                    {
                        _maxDeep--;
                    }
                }
                deep--;
            }
        }
        /// <summary>
        /// 获取剪枝后的叶节点列表
        /// </summary>
        /// <param name="leaves">剪枝前叶节点列表</param>
        /// <param name="node">被剪枝的节点</param>
        /// <returns></returns>
        private List<Node> GetPrunedLeaves(List<Node> leaves, Node node)
        {
            var dict = node.Children.ToDictionary(c => c.Value.id, c => c.Value);
            var list = leaves.Where(l => !dict.ContainsKey(l.id)).ToList();
            // 添加剪枝后的新叶节点
            var leaf = new Node() { id = node.id };
            leaf.parent = node.parent;
            leaf.deep = node.deep;
            leaf.Attr = node.Attr;
            leaf.count = node.count;
            leaf.classCount = node.classCount;
            int maxIdx = 0;
            double maxCount = node.classCount[0];
            for(int i = 0; i < node.classCount.Length; i++)
            {
                if(maxCount < node.classCount[i])
                {
                    maxIdx = i;
                    maxCount = node.classCount[i];
                }
            }
            leaf.Class = _classes[maxIdx];
            list.Add(leaf);
            return list;
        }

        /// <summary>
        /// 获取损失函数
        /// </summary>
        /// <param name="leaves"></param>
        /// <returns></returns>
        private double GetLoss(List<Node> leaves, double alpha = 1)
        {
            double sum = 0;
            foreach(var leaf in leaves)
            {
                double entropy = 0;
                foreach(var c in leaf.classCount)
                {
                    entropy -= c / leaf.count * Math.Log(c / leaf.count, 2);
                }
                sum += entropy * leaf.count;
            }
            return sum + leaves.Count * alpha;
        }
        /// <summary>
        /// 获取指定深度的前驱节点列表，即，节点深度为指定深度减1的节点列表
        /// </summary>
        /// <returns></returns>
        private List<Node> GetPrecNodes(int deep)
        {
            var list = new List<Node>();    // 结果列表
            //
            var dest = deep - 1;

            // bfs 遍历即可
            var queue = new Queue<Node>();
            queue.Enqueue(_root);

            while(queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.deep == dest)
                    list.Add(node);
                else if(node.deep < dest)
                {
                    if (node.Children != null)
                    {
                        foreach (var n in node.Children)
                        {
                            queue.Enqueue(n.Value);
                        }
                    }
                }

                //if (node.Children == null || node.Children.Count == 0)
                //    leaves.Add(node);
            }
            return list;
        }
        /// <summary>
        /// 获取初始的叶节点列表
        /// </summary>
        /// <returns></returns>
        private List<Node> GetInitLeaves()
        {
            // bfs 遍历即可
            var queue = new Queue<Node>();
            queue.Enqueue(_root);
            var leaves = new List<Node>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (node.Children == null || node.Children.Count == 0)
                    leaves.Add(node);
            }
            return leaves;
        }
    }

    public class Node
    {
        /// <summary>
        /// 节点唯一id
        /// </summary>
        public int id;
        /// <summary>
        /// 用于划分的属性名，叶节点为null
        /// </summary>
        public string Attr { get; set; }
        /// <summary>
        /// 节点分类，只有叶节点有分类值，内部节点为null
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// 根据属性的取值划分子空间，叶节点为null
        /// key为属性值，value为对应的子树的根结点，表示子空间
        /// </summary>
        public Dictionary<string, Node> Children { get; set; }

        /// <summary>
        /// 父节点，根节点的父节点为null
        /// </summary>
        public Node parent { get; set; }
        /// <summary>
        /// 对应父节点中Children的key值，父节点的划分属性对应的值
        /// </summary>
        public string attrVal;
        /// <summary>
        /// 深度，根节点深度为0
        /// </summary>
        public int deep;
        /// <summary>
        /// 每个分类的样本数量
        /// </summary>
        public double[] classCount;
        /// <summary>
        /// 节点覆盖的总样本数 = classCount.Sum()
        /// </summary>
        public double count;
    }

    public class Util
    {
        /// <summary>
        /// 判断枚举中各元素是否相等
        /// </summary>
        /// <param name="enums"></param>
        /// <returns></returns>
        public static bool IsSameElem(IEnumerable<string> enums)
        {
            string first = null;
            foreach (var @enum in enums)
            {
                if (first == null)
                    first = @enum;
                else if (first != @enum)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 给定数据集以及相关的属性和对应属性值，获取子数据集
        /// </summary>
        /// <param name="points"></param>
        /// <param name="attr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetSubRegion(List<Dictionary<string, string>> points, string attr, string val)
        {
            var list = new List<Dictionary<string, string>>();
            foreach (var point in points)
            {
                if (point[attr] == val)
                    list.Add(point);
            }
            return list;
        }

        /// <summary>
        /// 计算给定数据集系统以及指定属性作为随机变量的经验熵
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static double Entropy(List<Dictionary<string, string>> points, string attr)
        {
            var freqs = new Dictionary<string, double>();      // 指定属性，每个值出现的次数映射
            var count = points.Count;                           // 数据集大小
            foreach(var point in points)
            {
                var attrVal = point[attr];          // 数据点指定属性的值
                if (freqs.ContainsKey(attrVal))
                    freqs[attrVal] += 1.0;
                else
                    freqs[attrVal] = 1.0;
            }

            // 计算熵
            double h = 0;
            foreach(var f in freqs)
            {
                h += (-f.Value / count) * Math.Log(f.Value / count, 2);
            }
            return h;
        }
        /// <summary>
        /// 信息增益
        /// </summary>
        /// <param name="points"></param>
        /// <param name="attr"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double Gain(List<Dictionary<string, string>> points, string attr, string target)
        {
            // 计算条件经验熵，给定attr属性

            // 根据attr的取值，划分数据集空间，每个子空间的空间名与子数据集映射
            var subRegions = new Dictionary<string, List<Dictionary<string, string>>>();  // 子空间名为attr的值
            foreach(var point in points)
            {
                var attrVal = point[attr];
                if (subRegions.ContainsKey(attrVal))
                    subRegions[attrVal].Add(point);
                else
                    subRegions[attrVal] = new List<Dictionary<string, string>>() { point };
            }

            var count = (double)points.Count;       // 数据集总大小
            double hc = 0;
            foreach (var p in subRegions)
            {
                var subRegion_Prob = p.Value.Count / count;     // 属性值为attrVal的经验概率
                var subRegion_Entropy = Entropy(p.Value, target);   // 子系统对target这个随机变量的熵
                hc += subRegion_Prob * subRegion_Entropy;
            }

            var h = Entropy(points, target);    // 划分前的系统对target这个随机变量的经验熵
            return h - hc;          // 增益
        }
        /// <summary>
        /// 获取信息增益比
        /// 给定某属性，求数据集系统的信息增益比
        /// </summary>
        /// <param name="points">数据集</param>
        /// <param name="attr">给定的某属性</param>
        /// <param name="target">目标属性，即，分类，信息熵以分类作为随机变量来计算得到</param>
        /// <returns></returns>
        public static double GainRatio(List<Dictionary<string, string>> points, string attr, string target)
        {
            // 计算条件经验熵，给定attr属性

            // 根据attr的取值，划分数据集空间，每个子空间的空间名与子数据集映射
            var subRegions = new Dictionary<string, List<Dictionary<string, string>>>();  // 子空间名为attr的值
            foreach (var point in points)
            {
                var attrVal = point[attr];
                if (subRegions.ContainsKey(attrVal))
                    subRegions[attrVal].Add(point);
                else
                    subRegions[attrVal] = new List<Dictionary<string, string>>() { point };
            }

            var count = (double)points.Count;       // 数据集总大小
            double hc = 0;
            foreach (var p in subRegions)
            {
                var subRegion_Prob = p.Value.Count / count;     // 属性值为attrVal的经验概率
                var subRegion_Entropy = Entropy(p.Value, target);   // 子系统对target这个随机变量的熵
                hc += subRegion_Prob * subRegion_Entropy;
            }

            var h = Entropy(points, target);    // 划分前的系统对target这个随机变量的经验熵
            return 1 - hc/h;          // 信息增益比
        }

        /// <summary>
        /// 获取最佳划分的属性
        /// </summary>
        /// <param name="points"></param>
        /// <param name="attrs"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetBestAttr(List<Dictionary<string, string>> points, List<string> attrs, string target)
        {
            string bestAttr = null;
            double maxGain = 0;

            foreach(var attr in attrs)
            {
                if (attr == target) continue;

                var gain = Gain(points, attr, target);      // ID3算法，如果是C4.5算法，则将Gain函数替换为GainRatio函数
                if(maxGain < gain || bestAttr == null)
                {
                    bestAttr = attr;
                    maxGain = gain;
                }
            }
            return bestAttr;
        }

        /// <summary>
        /// 获取默认分类，默认分类是指具有分类值的数据点最多
        /// 在属性用完或者增益为0时，使用默认分类
        /// </summary>
        /// <param name="points"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetDefaultClass(List<Dictionary<string, string>> points, string target)
        {
            var freqs = new Dictionary<string, int>();  // 分类值与其数量的映射
            foreach(var point in points)
            {
                var val = point[target];
                if (freqs.ContainsKey(val))
                    freqs[val] += 1;
                else
                    freqs[val] = 1;
            }

            string cls = null;  // 分类
            int clsCount = 0;   // 分类对应的数量
            foreach(var p in freqs)
            {
                if(cls == null || p.Value > clsCount)
                {
                    cls = p.Key;
                    clsCount = p.Value;
                }
            }
            return cls;
        }
    }
}
