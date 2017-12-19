using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NLP.HAN
{
    public class Pinyin
    {
        /// <summary>
        /// 检测输入是否是有效的拼音，如是，返回首字母大写的（连接起来的）拼音，否则返回null
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <remarks>e.g. "jUansiwei" -> ["JuAnSiWei", "JuanSiWei"]</remarks>
        public static List<string> CheckPY(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var sb = new StringBuilder(input.Length);
            foreach(var c in input)
            {
                if (c >= 'A' && c <= 'Z')
                    sb.Append((char)(c + 32));
                else if (c >= 'a' && c <= 'z')
                    sb.Append(c);
                else
                    return null;
            }

            var pys = PYTrie.GuessPYs(sb.ToString());
            if (pys.Count > 0) return pys;
            return null;
        }
    }

    class PYTrie
    {
        private static PYNode _root = new PYNode() { children = new Dictionary<char, PYNode>(24) };
        
        private static PYTrie _instance = new PYTrie();
        public static PYTrie Instance { get { return _instance; } }
        static PYTrie()
        {
            foreach(var line in File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "res/CommonPinyin.txt"))
            {
                var segs = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < segs.Length; i++)
                    Insert(_root, segs[i], 0);
            }
        }


        private static void Insert(PYNode node, string text, int idx)
        {
            if (idx == text.Length - 1)      // 准备添加叶节点
            {
                node.children[text[idx]] = new PYNode() { IsLeaf = true };
            }
            else                            // 
            {
                PYNode subnode;
                if (!node.children.TryGetValue(text[idx], out subnode))
                {
                    subnode = new PYNode() { children = new Dictionary<char, PYNode>() };
                    node.children[text[idx]] = subnode;
                }
                if (subnode.children == null)
                    subnode.children = new Dictionary<char, PYNode>();
                Insert(subnode, text, idx + 1);
            }
        }

        private static void ExactMatch(string text, int idx, PYNode node, PYResultNode res_node)
        {
            if (idx == text.Length) return;      // match finished

            var c = text[idx++];
            PYNode subnode;
            if (!node.children.TryGetValue(c, out subnode))
            {
                return;                          // match failed
            }
            if (subnode.IsLeaf)
                res_node.children[idx] = new PYResultNode(idx) { parent = res_node };
            if (subnode.children == null)
                return;                         // match finished

            ExactMatch(text, idx, subnode, res_node);
        }

        /// <summary>
        /// 猜测拼音
        /// </summary>
        /// <param name="text">必须是小写</param>
        /// <returns></returns>
        public static List<string> GuessPYs(string text)
        {
            var root_resultnode = new PYResultNode(0);            // root of result node
            var queue = new Queue<PYResultNode>();
            queue.Enqueue(root_resultnode);

            var validLeaves = new List<PYResultNode>();
            var pys = new List<string>();

            while (queue.Count > 0)
            {
                var res_node = queue.Dequeue();
                if (res_node.start < text.Length)
                {
                    ExactMatch(text, res_node.start, _root, res_node);
                    if (res_node.children.Count > 0)
                    {
                        foreach (var p in res_node.children)
                            queue.Enqueue(p.Value);
                    }
                }
                else            // res_node.start == text.Length denotes it is a valid leaf
                {
                    validLeaves.Add(res_node);
                }
            }
            // 从根节点到叶节点的路径表示一种拼音划分方法，但是需要去掉不完整的划分方法（视为无效）            
            for (int i = 0; i < validLeaves.Count; i++)
            {
                var rn = validLeaves[i].parent;
                var origin_chars = text.ToCharArray();
                while (rn != null)
                {
                    origin_chars[rn.start] = (char)(origin_chars[rn.start] - 32);
                    rn = rn.parent;
                }
                pys.Add(new string(origin_chars));
            }
            return pys;
        }
    }

    class PYNode
    {
        public Dictionary<char, PYNode> children;

        public bool IsLeaf;
    }

    class PYResultNode
    {
        public int start;
        public Dictionary<int, PYResultNode> children = new Dictionary<int, PYResultNode>();
        public PYResultNode parent;
        public PYResultNode(int s)
        {
            start = s;
        }
    }
}
