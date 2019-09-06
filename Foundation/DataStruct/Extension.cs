using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foundation.datastructure
{
    static class Extension
    {
        public static List<BinNode<T>> ToList<T>(this BinTree<T> tree)
        {
            var list = new List<BinNode<T>>();

            Queue<BinTree<T>> queue = new Queue<BinTree<T>>();
            queue.Enqueue(tree);

            BinTree<T> subTree = null;

            while (queue.Any())
            {
                subTree = queue.Dequeue();
                list.Add(subTree.Node);

                if (subTree.LChild != null)
                    queue.Enqueue(subTree.LChild);
                if (subTree.RChild != null)
                    queue.Enqueue(subTree.RChild);
            }
            return list;
        }
        public static Queue<BinNode<T>> ToQueue<T>(this BinTree<T> tree)
        {
            var queue = new Queue<BinNode<T>>();

            Queue<BinTree<T>> queue1 = new Queue<BinTree<T>>();
            queue1.Enqueue(tree);

            BinTree<T> subTree = null;

            while (queue1.Any())
            {
                subTree = queue1.Dequeue();
                queue.Enqueue(subTree.Node);

                if (subTree.LChild != null)
                    queue1.Enqueue(subTree.LChild);
                if (subTree.RChild != null)
                    queue1.Enqueue(subTree.RChild);
            }
            return queue;
        }
    }
}
