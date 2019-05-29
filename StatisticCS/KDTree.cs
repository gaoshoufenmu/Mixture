using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace QZ.N2E.Tester
    {

        public class Point
        {
            /// <summary>
            /// 数据点的实数空间特征向量
            /// </summary>
            public double[] vector;
            public Point(double[] vector)
            {
                this.vector = vector;
            }

            /// <summary>
            /// 计算两个点之间的欧式距离
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public double Distance(Point other)
            {
                if (this.vector.Length != other.vector.Length) throw new Exception("");

                double squareSum = 0;
                for (int i = 0; i < vector.Length; i++)
                {
                    squareSum += Math.Pow((vector[i] - other.vector[i]), 2);
                }
                return Math.Sqrt(squareSum);
            }

            public bool EqualsTo(Point other)
            {
                if (vector.Length != other.vector.Length) return false;
                for (int i = 0; i < vector.Length; i++)
                {
                    if (vector[i] != other.vector[i])
                        return false;
                }
                return true;
            }
        }

        public class Range
        {
            public double[,] boundaries;

            public static Range CreateInf(int dim)
            {
                var r = new Range(dim);
                for (int i = 0; i < dim; i++)
                {
                    r.boundaries[i, 0] = double.MinValue;
                    r.boundaries[i, 1] = double.MaxValue;
                }
                return r;
            }
            public Range(int dim)
            {
                boundaries = new double[dim, 2];
            }

            public Range(double[,] boundaries)
            {
                this.boundaries = boundaries;
            }

            public Range Intersect(Range r)
            {
                if (r.boundaries.Length != this.boundaries.Length) throw new Exception("");

                var range = new Range(this.boundaries.Length);
                for (int i = 0; i < this.boundaries.Length; i++)
                {
                    var leftMax = this.boundaries[i, 0] > r.boundaries[i, 0] ? this.boundaries[i, 0] : r.boundaries[i, 0];
                    var rightMin = this.boundaries[i, 1] < r.boundaries[i, 1] ? this.boundaries[i, 1] : r.boundaries[i, 1];
                    range.boundaries[i, 0] = leftMax;
                    range.boundaries[i, 1] = rightMin;
                }
                return range;
            }

            /// <summary>
            /// 经过点并垂直于坐标轴切割空间，并获取左侧（轴上较小值）空间
            /// </summary>
            /// <param name="p"></param>
            /// <param name="axis">轴标号，从0开始</param>
            /// <returns></returns>
            public static Range LeftRange(Point p, int axis)
            {
                if (axis >= p.vector.Length) throw new Exception("");

                var range = CreateInf(p.vector.Length);
                range.boundaries[axis, 1] = p.vector[axis];
                return range;
            }
            public static Range RightRange(Point p, int axis)
            {
                if (axis >= p.vector.Length) throw new Exception("");

                var range = CreateInf(p.vector.Length);
                range.boundaries[axis, 0] = p.vector[axis];
                return range;
            }
        }

        public class TreeNode
        {
            /// <summary>
            /// related point
            /// </summary>
            public Point point;
            /// <summary>
            /// perpendicular on which axis the splitted hyperplane is
            /// </summary>
            public int axis;

            public TreeNode parent;
            public TreeNode left;
            public TreeNode right;

            public Range range;
            public bool isVisited;

        }
        public class KDTree
        {
            /// <summary>
            /// root of this K-D tree
            /// </summary>
            public TreeNode root;
            /// <summary>
            /// dimension
            /// </summary>
            public int dim;
            /// <summary>
            /// Constructor according to a given list of trainning points
            /// </summary>
            /// <param name="points"></param>
            public KDTree(List<Point> points)
            {
                dim = points[0].vector.Length;
                root = new TreeNode() { range = Range.CreateInf(dim) };

                RecursivelyConstruct(root, points, 0);
            }

            /// <summary>
            /// 递归构造K-D树，直到所有数据点被分配完成
            /// </summary>
            /// <param name="node">当前需要确定对应哪个数据点的节点</param>
            /// <param name="points">当前未分配的数据点</param>
            /// <param name="depth">当前节点的深度（root为0）</param>
            private void RecursivelyConstruct(TreeNode node, List<Point> points, int depth)
            {
                if (points.Count == 1)
                {
                    node.point = points[0];
                    return;
                }

                var axis = GetAxis4SplitByVar(points);
                var m = GetMedianIndex(points, axis);
                node.axis = axis;
                node.point = points[m];

                if (m > 0)   // has left subregion
                {
                    var t = CreateChildNode(node, true, m, axis, points);
                    RecursivelyConstruct(t.Item1, t.Item2, depth + 1);
                }
                if (m < points.Count - 1)    // has right subregion
                {
                    var t = CreateChildNode(node, false, m, axis, points);
                    RecursivelyConstruct(t.Item1, t.Item2, depth + 1);
                }
            }


            /// <summary>
            /// 搜索与给定点最近的点
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public Point SearchNearestNode(Point p)
            {
                var stack = new Stack<TreeNode>();          // to store those visited nodes
                DownRecurseSearch(p, root, stack);

                var leaf = stack.Pop();
                var node = Traceback(p, leaf, stack);
                return node.point;
            }

            private List<Tuple<TreeNode, double>> _priorities = new List<Tuple<TreeNode, double>>();
            /// <summary>
            /// 按priority升序排序插入
            /// </summary>
            /// <param name="node"></param>
            /// <param name="priority"></param>
            private void InsertByPriority(TreeNode node, double priority)
            {
                if (_priorities.Count == 0)
                {
                    _priorities.Add(new Tuple<TreeNode, double>(node, priority));
                }
                else
                {
                    for (int i = 0; i < _priorities.Count; i++)
                    {
                        if (_priorities[i].Item2 >= priority)
                        {
                            _priorities.Insert(i, new Tuple<TreeNode, double>(node, priority));
                            break;
                        }
                    }
                }
            }
            private double GetPriority(TreeNode node, Point p, int axis) => Math.Abs(node.point.vector[axis] - p.vector[axis]);

            /// <summary>
            /// 最大检测次数
            /// </summary>
            public int max_nn_chks = 0x1000;
            /// <summary>
            /// 搜索k近邻点
            /// </summary>
            /// <param name="p"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public List<TreeNode> BBFSearchKNearest(Point p, int k)
            {
                var list = new List<BBFData>();    //
                var pq = new MinPQ();
                pq.insert(new PQNode(root, 0));
                int t = 0;
                while (pq.nodes.Count > 0 && t < max_nn_chks)
                {
                    var expl = pq.pop_min_default().data;
                    expl = Explore2Leaf(expl, p, pq);

                    var bbf = new BBFData(expl, expl.point.Distance(p));
                    insert(list, k, bbf);

                    t++;
                }
                return list.Select(l => l.data).ToList();
            }
            /// <summary>
            /// 向下访问叶节点，并将slide添加到优先列表中
            /// </summary>
            /// <param name="node"></param>
            /// <param name="p"></param>
            /// <param name="pq"></param>
            /// <returns></returns>
            private TreeNode Explore2Leaf(TreeNode node, Point p, MinPQ pq)
            {
                TreeNode unexpl;
                var expl = node;
                TreeNode prev;
                while (expl != null && (expl.left != null || expl.right != null))
                {
                    prev = expl;
                    var axis = expl.axis;
                    var val = expl.point.vector[axis];

                    if (p.vector[axis] <= val)
                    {
                        unexpl = expl.right;
                        expl = expl.left;
                    }
                    else
                    {
                        unexpl = expl.left;
                        expl = expl.right;
                    }
                    if (unexpl != null)
                    {
                        pq.insert(new PQNode(unexpl, Math.Abs(val - p.vector[axis])));
                    }
                    if (expl == null)
                    {
                        return prev;
                    }
                }
                return expl;
            }
            /// <summary>
            /// 将节点按距离插入列表中
            /// </summary>
            /// <param name="list"></param>
            /// <param name="k"></param>
            /// <param name="bbf"></param>
            private void insert(List<BBFData> list, int k, BBFData bbf)
            {
                if (list.Count == 0)
                {
                    list.Add(bbf);
                    return;
                }

                int ret = 0;
                int oldCount = list.Count;
                var last = list[list.Count - 1];
                var df = bbf.d;
                var dn = last.d;
                if (df >= dn)        // bbf will be appended to list
                {
                    if (oldCount == k)     // already has k nearest neighbors, nothing should be done
                    {
                        return;
                    }
                    list.Add(bbf);      // append directively
                    return;
                }

                // bbf will be inserted into list internally

                if (oldCount < k)
                {
                    // suppose bbf be inserted at idx1, all elements after idx1 should be moved 1 backward respectively
                    // first we move the last element
                    list.Add(last);
                }
                // from backer to former, move related elements
                int i = oldCount - 2;
                while (i > -1)
                {
                    if (list[i].d <= df)
                        break;

                    list[i + 1] = list[i];      // move backward
                    i--;
                }
                i++;
                list[i] = bbf;
            }

            public Point BBFSearchNearestNode(Point p)
            {
                var rootPriority = GetPriority(root, p, root.axis);
                InsertByPriority(root, rootPriority);
                var nearest = root;

                TreeNode topNode = null;        // 优先级最高的节点
                TreeNode curNode = null;
                while (_priorities.Count > 0)
                {
                    topNode = _priorities[0].Item1;
                    _priorities.RemoveAt(0);

                    while (topNode != null)
                    {
                        if (topNode.left != null || topNode.right != null)
                        {
                            var axis = topNode.axis;
                            if (p.vector[axis] <= topNode.point.vector[axis])
                            {
                                // wanna to go down left child node 
                                if (topNode.right != null)                                       // 将右子节点添加到优先列表
                                {
                                    InsertByPriority(topNode.right, GetPriority(topNode.right, p, axis));
                                }
                                topNode = topNode.left;
                            }
                            else
                            {
                                // wanna to go down right child node
                                if (topNode.left != null)
                                {
                                    InsertByPriority(topNode.left, GetPriority(topNode.left, p, axis));
                                }
                                topNode = topNode.right;
                            }
                        }
                        else
                        {
                            curNode = topNode;
                            topNode = null;
                        }

                        if (curNode != null && p.Distance(curNode.point) < p.Distance(nearest.point))        // find a nearer node
                        {
                            nearest = curNode;
                        }
                    }
                }
                return nearest.point;
            }

            /// <summary>
            /// 向下递归访问节点直到遇到叶节点
            /// 考虑了某一个子节点为空的情况
            /// </summary>
            /// <param name="p"></param>
            /// <param name="n"></param>
            /// <param name="stack"></param>
            private void DownRecurseSearch(Point p, TreeNode n, Stack<TreeNode> stack)
            {
                stack.Push(n);
                n.isVisited = true;
                if (n.left == null && n.right == null) return;      // leaf reached

                if (GoDownLeftFirst(p, n))                          // go down left as soon as posssible
                {
                    if (n.left != null && !n.left.isVisited)
                        DownRecurseSearch(p, n.left, stack);
                    else if (n.right != null && !n.right.isVisited)
                        DownRecurseSearch(p, n.right, stack);
                }
                else                                                // go down right as soon as posssible
                {
                    if (n.left != null && !n.left.isVisited)
                        DownRecurseSearch(p, n.left, stack);
                    else if (n.right != null && !n.right.isVisited)
                        DownRecurseSearch(p, n.right, stack);
                }
            }

            /// <summary>
            /// 继续向下访问节点的子节点，true -> left child node; false -> right child node
            /// </summary>
            /// <param name="p"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            private bool GoDownLeftFirst(Point p, TreeNode n)
            {
                var axis = n.axis;
                return p.vector[axis] < n.point.vector[axis];
            }

            /// <summary>
            /// 向上回溯查找最近邻点
            /// </summary>
            /// <param name="p">目标点</param>
            /// <param name="n">当前最近邻点</param>
            /// <param name="stack">已访问过的节点</param>
            /// <returns></returns>
            private TreeNode Traceback(Point p, TreeNode n, Stack<TreeNode> stack)
            {
                if (stack.Count == 0)
                {
                    n.isVisited = false;
                    return n;
                }
                var parent = stack.Pop();       // parent node of the current node n
                parent.isVisited = false;
                // check current node and its parent which is nearer to destination p?
                var dn = p.Distance(n.point);       // distance between n and p, and let it be the currently nearest distance
                var dp = p.Distance(parent.point);  // distance between parent and p
                if (dp < dn)
                {
                    dn = dp;            // update the currently nearest distance
                    n = parent;         // update the currently nearest node
                }

                if (Intersect(p, dn, parent))   // 如果p为球心，当前最短距离为半径的超球体与父节点的切割超平明相交，则有必要去父节点的另一个空间向下递归查找最近邻点
                {
                    // 当前父节点的另一个子空间，考虑了另一个子空间可能不存在数据点的情况
                    TreeNode other = null;
                    if (parent.left != null && !parent.left.isVisited)
                        other = parent.left;
                    else if (parent.right != null && !parent.right.isVisited)
                        other = parent.right;

                    if (other != null)
                    {
                        var localStack = new Stack<TreeNode>();
                        DownRecurseSearch(p, other, localStack);
                        var localNode = Traceback(p, localStack.Pop(), localStack);     // get the nearest node in this local sub region
                                                                                        // update the min distance and nearest node if needed
                        var localDist = p.Distance(localNode.point);
                        if (localDist < dn)
                        {
                            dn = localDist;
                            n = localNode;
                        }
                    }

                }
                n.isVisited = false;


                // go on up-traceback
                return Traceback(p, n, stack);
            }

            /// <summary>
            /// 以p为球心，radis为半径的超球体，是否与经过点n且与垂直于n的axis的超平面（切割超平面）相交
            /// </summary>
            /// <param name="p">目标点</param>
            /// <param name="radis">当前的最近距离</param>
            /// <param name="n">被考察的节点</param>
            /// <returns></returns>
            private bool Intersect(Point p, double radis, TreeNode n)
            {
                var axis = n.axis;
                return Math.Abs(p.vector[axis] - n.point.vector[axis]) < radis;
            }

            /// <summary>
            /// 创建子节点
            /// </summary>
            /// <param name="parent">父节点</param>
            /// <param name="isLeft">是否为左子节点</param>
            /// <param name="m">父节点对应空间的数据集中位数索引</param>
            /// <param name="axis">作用维度</param>
            /// <param name="points">父节点对应空间的数据集</param>
            /// <returns></returns>
            private Tuple<TreeNode, List<Point>> CreateChildNode(TreeNode parent, bool isLeft, int m, int axis, List<Point> points)
            {
                var subRegion = isLeft ? points.Take(m).ToList() : points.Skip(m + 1).ToList();
                var node = new TreeNode();
                node.parent = node;
                var range = isLeft ? Range.LeftRange(points[m], axis) : Range.RightRange(points[m], axis);
                node.range = parent.range.Intersect(range);
                if (isLeft)
                    parent.left = node;
                else
                    parent.right = node;
                return new Tuple<TreeNode, List<Point>>(node, subRegion);
            }
            /// <summary>
            /// 选择方差最大的那个维度
            /// </summary>
            /// <param name="points"></param>
            /// <returns></returns>
            private int GetAxis4SplitByVar(List<Point> points)
            {
                var dim = points[0].vector.Length;
                var aves = new double[dim];

                double max = 0;
                int axis = 0;
                for (int i = 0; i < dim; i++)
                {
                    aves[i] = points.Sum(p => p.vector[i]) / dim;
                    var variance = points.Sum(p => Math.Pow((p.vector[i] - aves[i]), 2)) / dim;
                    if (max < variance)
                    {
                        max = variance;
                        axis = i;
                    }
                }
                return axis;
            }

            /// <summary>
            /// 根据深度，轮选维度
            /// </summary>
            /// <param name="depth"></param>
            /// <param name="dim"></param>
            /// <returns></returns>
            private int GetAxis4SplitByDep(int depth, int dim) => depth % dim;

            /// <summary>
            /// Given a list of points and the concerned axis, get the index at which the point has a median on the concerned axis
            /// </summary>
            /// <param name="points"></param>
            /// <param name="axis"></param>
            /// <returns></returns>
            private int GetMedianIndex(List<Point> points, int axis)
            {
                QuickSort(points, 0, points.Count - 1, axis);
                return points.Count / 2;
            }
            private void QuickSort(List<Point> points, int start, int end, int axis)
            {
                if (start < end)
                {
                    int s = start;
                    int e = end;
                    var pivot_i = (start + end) / 2;
                    var pivot_v = points[pivot_i].vector[axis];
                    while (s < e)
                    {
                        while (s < e && points[s].vector[axis] <= pivot_v)
                        {
                            s++;
                        }
                        while (e > s && points[e].vector[axis] >= pivot_v)
                        {
                            e--;
                        }
                        if (s < e)
                        {
                            var temp = points[s];
                            points[s] = points[e];
                            points[e--] = temp;
                        }
                    }
                    QuickSort(points, start, s - 1, axis);
                    QuickSort(points, s + 1, end, axis);
                }
            }
        }

        public class BBFData
        {
            public TreeNode data;
            /// <summary>
            /// 节点与目标点的距离
            /// </summary>
            public double d;

            public BBFData(TreeNode data, double d)
            {
                this.data = data;
                this.d = d;
            }
        }

        public class PQNode
        {
            public TreeNode data;
            /// <summary>
            /// 目标点与当前节点的超平面的距离
            /// </summary>
            public double d;

            public PQNode(TreeNode data, double d)
            {
                this.data = data;
                this.d = d;
            }
        }

        public class MinPQ
        {
            public List<PQNode> nodes;

            public void insert(PQNode node)
            {
                nodes.Add(node);

                int i = nodes.Count - 1;
                int p = parent(i);
                PQNode tmp;
                while (i > 0 && p >= 0 && nodes[i].d < nodes[p].d)
                {
                    tmp = nodes[p];
                    nodes[p] = nodes[i];
                    nodes[i] = tmp;
                    i = p;
                    p = parent(i);
                }
            }

            public PQNode get_min_default() => nodes.Count > 0 ? nodes[0] : null;
            public PQNode pop_min_default()
            {
                if (nodes.Count == 0) return null;

                var ret = nodes[0];
                nodes[0] = nodes[nodes.Count - 1];
                nodes.RemoveAt(nodes.Count - 1);
                restore_minpq_order(0, nodes.Count);

                return ret;
            }

            private void restore_minpq_order(int i, int n)
            {
                int l = left(i);
                int r = right(i);
                int min = i;

                if (l < n && nodes[l].d < nodes[i].d)
                    min = l;
                if (r < n && nodes[r].d < nodes[min].d)
                    min = r;
                if (min != i)
                {
                    var tmp = nodes[min];
                    nodes[min] = nodes[i];
                    nodes[i] = tmp;
                }
            }

            public static int parent(int i) => (i - 1) / 2;
            public static int right(int i) => 2 * (i + 1);
            public static int left(int i) => 2 * i + 1;
        }
    }

}
