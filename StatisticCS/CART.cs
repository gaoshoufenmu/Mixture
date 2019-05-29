using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StatisticCS
{
    class CARTTree
    {
        private CARTNode _root;
        public CARTNode Root { get { return _root; } }

        /// <summary>
        /// 根据样本数据创建CART决策树
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CARTTree Create(CARTData data)
        {
            var tree = new CARTTree() { _root = new CARTNode() };
            var attrIdxs = Enumerable.Range(0, data.J - 1).ToList();   // 输入属性索引列表 
            Create(tree._root, attrIdxs, data.trainSet);
            return Prune(tree, data);
        }
        /// <summary>
        /// 生成决策树
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attrIdxs"></param>
        /// <param name="points"></param>
        private static void Create(CARTNode node, List<int> attrIdxs, List<CARTPoint> points)
        {
            node.region = points;
            // 根据CART分裂策略，分裂后的区域内样本点数量至少为1，不可能为0
            if (points.Count == 1)
            {
                // 如果为1，则不再分裂，直接设置为叶节点
                node.output = points[0].vals.LastOrDefault();
            }
            else
            {
                var ave = points.Sum(p => p.vals.LastOrDefault()) / points.Count;
                // 没有可用于分裂的属性，则设置叶节点
                // 输出值的估计为区域中样本点输出值的均值
                if (attrIdxs.Count == 0)
                {
                    node.output = ave;
                }
                else
                {
                    // 先计算整体的样本点的方差，如果小于阈值，则不分裂
                    double squareErr = 0;
                    foreach(var p in points)
                    {
                        squareErr += (p.vals.LastOrDefault() - ave) * (p.vals.LastOrDefault() - ave);
                    }
                    if (squareErr < ave / 1000)
                    {
                        // 如果方差小于一个阈值，则停止分裂，这里为了简单起见，阈值hardcode
                        node.output = ave;
                    }
                    else
                    {
                        TempResult minTemp = null;  // 最小平方误差
                        int minJ = 0;               // 对应的分裂属性索引
                        for (var i = 0; i < attrIdxs.Count; i++)
                        {
                            var j = attrIdxs[i];        // 输入属性的索引
                            var temp = CARTUtil.SquareError(j, points);
                            if (minTemp == null || temp.lossVal < minTemp.lossVal)
                            {
                                minTemp = temp;
                                minJ = j;
                            }
                        }

                        // 得到最小平方误差
                        node.j = minJ;
                        node.splitVal = minTemp.splitVal;

                        node.left = new CARTNode() { parent = node };
                        node.right = new CARTNode() { parent = node };

                        var leftAttrIdxs = attrIdxs.Where(idx => idx != minJ).Select(idx => idx).ToList();
                        var rightAttrIdxs = attrIdxs.Where(idx => idx != minJ).Select(idx => idx).ToList();
                        // 递归创建左右子节点
                        Create(node.left, leftAttrIdxs, minTemp.region1);
                        Create(node.right, rightAttrIdxs, minTemp.region2);
                    }
                }
            }
        }

        /// <summary>
        /// 剪枝
        /// </summary>
        /// <param name="tree">完全生长的决策树</param>
        /// <param name="data">提供验证数据集</param>
        /// <returns></returns>
        private static CARTTree Prune(CARTTree tree, CARTData data)
        {
            // 获取最优子树序列
            var list = new List<CARTTree>() { tree };    // 最优子树序列
            var curTree = tree;
            while(!Is_ThreeNode_Tree(curTree))
            {
                curTree = GetSubTree(curTree);
                list.Add(curTree);
            }

            // 使用验证集获得最终的最优子树

            // 验证集，最小平方误差
            double min_err = double.MaxValue;
            CARTTree best_tree = null;      // 最小平方误差对应的最优子树
            for(int i = 0; i < list.Count; i++)
            {
                var sub_tree = list[i];
                double squareErrSum = 0;
                for(int k = 0; k < data.verifySet.Count; k++)
                {
                    squareErrSum += Judge(data.verifySet[k], sub_tree);
                }
                if(min_err > squareErrSum)
                {
                    min_err = squareErrSum;
                    best_tree = sub_tree;
                }
            }
            return best_tree;
        }

        /// <summary>
        /// 决策：根据输入计算模型输出值
        /// </summary>
        /// <param name="point">样本点</param>
        /// <param name="tree">决策树</param>
        /// <returns></returns>
        public static double Judge(CARTPoint point, CARTTree tree) => Judge(point, tree._root);

        /// <summary>
        /// 递归获取模型输出值
        /// </summary>
        /// <param name="point"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static double Judge(CARTPoint point, CARTNode node)
        {
            if (node.left != null)
                return node.output;
            else
            {
                if (point.vals[node.j] < node.splitVal)
                    return Judge(point, node.left);
                else
                    return Judge(point, node.right);
            }
        }

        /// <summary>
        /// 是否是三节点组成的树， 即一个根节点加两个子节点
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private static bool Is_ThreeNode_Tree(CARTTree tree)
        {
            var left = tree._root.left;
            var right = tree._root.right;
            if (left.left != null || right.left != null)
                return false;
            return true;
        }

        private static CARTTree GetSubTree(CARTTree tree)
        {
            var root = tree._root;
            var stack = new Stack<CARTNode>();
            AccessNonLeaf(root, stack);
            double minAlpha = double.MaxValue;  // 最小alpha
            CARTNode minNode = null;            // 最小alpha对应的内部节点
            while(stack.Count > 0)
            {
                var t = stack.Pop();    // 某一内部节点
                // 以 t 为根节点的子树的所有叶节点
                var leaves_t = CARTUtil.GetLeaves(t);
                // 以 t 为单节点，平方误差为
                var var_t = CARTUtil.GetVar(t);
                // 以 t 为根节点的子树，平方误差为
                double var_subtree = 0;
                for(int i = 0; i < leaves_t.Count; i++)
                {
                    var leaf = leaves_t[i];
                    var_subtree += CARTUtil.GetVar(leaf);
                }
                var alpha_t = (var_t - var_subtree) / (leaves_t.Count - 1);
                if(minAlpha > alpha_t)
                {
                    minAlpha = alpha_t;
                    minNode = t;
                }
            }
            // 获得最小的alpha，则对此节点剪枝，
            // 需要注意的是，由于最终要生成一个子树序列，所以，我们不对原来的树剪枝，而是复制一个树并剪枝
            return PrunedClone(tree, minNode);
        }

        private static CARTTree PrunedClone(CARTTree tree, CARTNode node)
        {
            var queue = new Queue<CARTNode>();          // 原始树队列
            queue.Enqueue(tree._root);
            var root_1 = new CARTNode();                // 新树根节点
            var queue_1 = new Queue<CARTNode>();        // 同步队列
            queue_1.Enqueue(root_1);

            while(queue.Count > 0)
            {
                var n = queue.Dequeue();
                var n_1 = queue_1.Dequeue();

                if(n == node)   // 遇到需要被剪枝的内部节点，则需要剪枝为叶节点
                {
                    // 设置叶节点的必要字段
                    n_1.output = n.region.Sum(p => p.vals.LastOrDefault()) / n.region.Count;
                    n_1.region = n.region;
                }
                else
                {
                    n_1.Update(n);      // 更新节点的固有属性（字段）
                    if(n.left != null)
                    {
                        // 非叶节点
                        n_1.left = new CARTNode() { parent = n_1 };
                        n_1.right = new CARTNode() { parent = n_1 };

                        queue.Enqueue(n.left);
                        queue.Enqueue(n.right);

                        queue_1.Enqueue(n_1.left);
                        queue_1.Enqueue(n_1.right);
                    }
                    // else，是叶节点，无需其他操作
                }
            }
            return new CARTTree() { _root = root_1 };
        }

        private static void AccessNonLeaf(CARTNode node, Stack<CARTNode> stack)
        {
            // node有子节点，且不是根结点，说明是内部叶节点
            if(node.left != null && node.parent != null)    
            {
                stack.Push(node);
                AccessNonLeaf(node.left, stack);
                AccessNonLeaf(node.right, stack);
            }
        }
    }

    public class CARTNode
    {
        ///// <summary>
        ///// 分裂属性的值类型：离散or连续？
        ///// </summary>
        //public ValType valType;
        /// <summary>
        /// 分裂属性的索引
        /// </summary>
        public int j = -1;
        /// <summary>
        /// 分裂点值
        /// </summary>
        public double splitVal;

        /// <summary>
        /// 父节点，剪枝阶段用到
        /// </summary>
        public CARTNode parent;

        /// <summary>
        /// 输出值，叶节点才有
        /// </summary>
        public double output = double.MinValue;

        public CARTNode() { }

        public void Update(CARTNode node)
        {
            this.j = node.j;
            this.splitVal = node.splitVal;
            this.output = node.output;
            this.region = node.region;
        }


        public List<CARTPoint> region;

        //-------------
        // 一个节点要么为叶节点，要么为非叶节点，根据这里的分裂逻辑，非叶节点必定是有两个子节点
        //---------------

        /// <summary>
        /// 左子节点，对应切点点值的左侧
        /// </summary>
        public CARTNode left;
        /// <summary>
        /// 右子节点，对应切分点值的右侧
        /// </summary>
        public CARTNode right;
        
    }
    /// <summary>
    /// 样本数据点
    /// </summary>
    public class CARTPoint
    {
        /// <summary>
        /// 数据点各属性的值，最后一个属性表示输出
        /// 如果是离散型属性，将离散型值映射为实数
        /// </summary>
        public double[] vals;
        public CARTPoint(int d)
        {
            vals = new double[d];
        }
    }

    public class CARTData
    {
        /// <summary>
        /// 属性数量，包括输出
        /// </summary>
        public int J;
        /// <summary>
        /// 训练数据集
        /// </summary>
        public List<CARTPoint> trainSet = new List<CARTPoint>();
        /// <summary>
        /// 离散值到实数的映射
        /// key: 属性索引, value: 离散值到实数的映射
        /// </summary>
        public Dictionary<int, Dictionary<string, double>> disc2Real = new Dictionary<int, Dictionary<string, double>>();
        /// <summary>
        /// key:属性索引,value: 实数到离散值的映射，以实数为索引得到的elem值为属性离散值
        /// </summary>
        public Dictionary<int, string[]> real2Disc = new Dictionary<int, string[]>();
        /// <summary>
        /// 属性名和对应的值类型
        /// </summary>
        public List<string> attrNames = new List<string>();
        /// <summary>
        /// 验证数据集
        /// </summary>
        public List<CARTPoint> verifySet = new List<CARTPoint>();


        public void Init(string path)
        {
            var lines = File.ReadAllLines(path);
            int flag = 0;   // 1: train-data; 1: verify-data
            foreach(var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if(line.StartsWith("@ATTRIBUTE"))
                {
                    var segs = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    attrNames.Add(segs[1]);
                    
                    if(segs[2] != "cont")
                    {
                        var vals = segs[3].Split('/');
                        real2Disc.Add(J, vals);

                        var dict = new Dictionary<string, double>();
                        for(int i = 0; i < vals.Length; i++)
                        {
                            dict.Add(vals[i], i);
                        }
                        disc2Real.Add(J, dict);
                    }

                    J++;
                }
                else if(line.StartsWith("@train-data"))
                {
                    flag = 1;
                }
                else if(line.StartsWith("@verify-data"))
                {
                    flag = 2;
                }
                else
                {
                    
                    var segs = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var point = new CARTPoint(segs.Length);
                    for(int i = 0; i < segs.Length; i++)
                    {
                        double d;
                        if(!double.TryParse(segs[i], out d))
                        {
                            // 离散值，获取对应的映射实数
                            d = disc2Real[i][segs[i]];
                        }
                        point.vals[i] = d;
                    }
                    if(flag == 1)
                    {
                        // 训练数据
                        trainSet.Add(point);
                    }
                    else
                    {
                        // 验证数据
                        verifySet.Add(point);
                    }
                }
            }
        }
    }

    public class CARTUtil
    {
        /// <summary>
        /// 获取以指定节点为根结点的子树中的所有叶节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<CARTNode> GetLeaves(CARTNode node)
        {
            var list = new List<CARTNode>();
            var queue = new Queue<CARTNode>();
            queue.Enqueue(node);
            while(queue.Count > 0)
            {
                var n = queue.Dequeue();
                if (n.left == null)
                    list.Add(n);
                else
                {
                    queue.Enqueue(n.left);
                    queue.Enqueue(n.right);
                }
            }
            return list;
        }

        /// <summary>
        /// 获取方差，作为回归问题中的预测误差
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double GetVar(CARTNode node)
        {
            double ave = 0;
            if (node.left != null)
                ave = node.region.Sum(p => p.vals.LastOrDefault()) / node.region.Count;
            else
                ave = node.output;
            return node.region.Sum(p => Math.Pow(p.vals.LastOrDefault() - ave, 2));
        }

        /// <summary>
        /// 给定切分变量j，计算最小平方误差
        /// 切分点根据样本中相邻切分属性值的中间值逐一选择
        /// </summary>
        /// <param name="j">切分属性的索引</param>
        /// <param name="points">区域中的数据点集合</param>
        /// <returns></returns>
        public static TempResult SquareError(int j, List<CARTPoint> points)
        {
            var t_idx = points[0].vals.Length - 1;
            CARTSort(points, j);        // 根据j属性值升序排序

            var list = GetSplitVals(points, j);
            double minError = double.MaxValue;
            double split_val = 0;
            List<CARTPoint> region_1 = null;
            List<CARTPoint> region_2 = null;
            for(int i = 0; i < list.Count; i++)
            {
                var tuple = list[i];
                var region1 = points.Take(tuple.Item1 + 1).ToList();
                var region2 = points.Skip(tuple.Item1 + 1).ToList();
                var c1 = EstimateY(region1);
                var c2 = EstimateY(region2);

                double squreError = 0;
                foreach(var p in region1)
                {
                    squreError += (p.vals[t_idx] - c1) * (p.vals[t_idx] - c1);
                }
                foreach (var p in region2)
                {
                    squreError += (p.vals[t_idx] - c2) * (p.vals[t_idx] - c2);
                }
                if (minError > squreError)
                {
                    minError = squreError;
                    split_val = tuple.Item2;
                    region_1 = region1;
                    region_2 = region2;
                }
            }
            return new TempResult() { splitVal = split_val, region1 = region_1, region2 = region_2 };
        }

        private static double EstimateY(List<CARTPoint> points)
        {
            var t_idx = points[0].vals.Length - 1;
            return points.Sum(p => p.vals[t_idx]) / points.Count;
        }
        /// <summary>
        /// 根据 j属性，获取排序后的样本的切分位置，比如切分位置为i，则切分为{e|idx &lte; i -1, idx >= 0}, {e|idx > i,  idx &lt; Count}
        /// 增加Item2，表示切分点值
        /// </summary>
        /// <param name="points"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private static List<Tuple<int, double>> GetSplitVals(List<CARTPoint> points, int j)
        {
            var list = new List<Tuple<int, double>>();
            var t_idx = points[0].vals.Length - 1;      // 输出属性的索引
            //double prev = double.MinValue;      // 上一个样本点的 j 属性值
            for(int i = 0; i < points.Count; i++)
            {
                var start = points[i];
                //prev = start.vals[j];
                for(int k = i + 1; k < points.Count;k++)
                {
                    var cursor = points[k];
                    // 如果输出属性相等则这两个相邻样本点之间不设置切分点，从而减少计算量
                    if (cursor.vals[t_idx] == start.vals[t_idx]) continue;

                    // 如果输出属性不相等，则
                    var idx = k - 1;
                    var s = (cursor.vals[j] + points[k - 1].vals[j]) / 2;
                    list.Add(new Tuple<int, double>(idx, s));
                    
                }
            }
            return list;
        }
        /// <summary>
        /// 根据j属性值升序排序
        /// </summary>
        /// <param name="points"></param>
        /// <param name="j"></param>
        private static void CARTSort(List<CARTPoint> points, int j)
        {
            var t_idx = points[0].vals.Length - 1;  // 输出属性的索引
            // 插入排序，故意避免递归
            for(int i = 1; i < points.Count; i++)
            {
                if(points[i-1].vals[j] > points[i].vals[j])
                {
                    var temp = points[i];
                    int k = i;
                    while(k >0 && points[k -1].vals[j] > temp.vals[j])
                    {
                        points[k] = points[k - 1];
                        k--;
                    }
                    while(k > 0 && points[k-1].vals[j]== temp.vals[j] && points[k - 1].vals[t_idx] > temp.vals[t_idx])  // 需要进行二级排序
                    {
                        points[k] = points[k - 1];
                        k--;
                    }
                    points[k] = temp;
                }
                // 如果 j 属性值相等，则进行二级排序，按输出属性值升序排序
                else if(points[i-1].vals[j] == points[i].vals[j])    
                {
                    if(points[i-1].vals[t_idx] > points[i].vals[t_idx])
                    {
                        var temp = points[i];
                        int k = i;
                        while(k > 0 && points[k-1].vals[j] == temp.vals[j] && points[k-1].vals[t_idx] > temp.vals[t_idx])
                        {
                            points[k] = points[k - 1];
                            k--;
                        }
                        points[k] = temp;
                    }
                }
            }
        }
    }

    public class TempResult
    {
        /// <summary>
        /// 损失函数值
        /// </summary>
        public double lossVal;
        /// <summary>
        /// 分裂点值
        /// </summary>
        public double splitVal;
        /// <summary>
        /// 子区域1
        /// </summary>
        public List<CARTPoint> region1;
        /// <summary>
        /// 子区域2
        /// </summary>
        public List<CARTPoint> region2;

    }

    public enum ValType
    {
        /// <summary>
        /// 连续型
        /// </summary>
        cont,
        /// <summary>
        /// 离散型
        /// </summary>
        disc
    }
}
