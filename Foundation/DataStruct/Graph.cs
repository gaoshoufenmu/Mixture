using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foundation.datastructure
{
    public class BinTree<T>
    {
        public BinNode<T> Node { get; set; }
        public BinTree<T> LChild;
        public BinTree<T> RChild;

        public static BinTree<U> Build<U>(U[] array)
        {
            if (array == null || array.Length < 1)
                throw new ArgumentException("Array length of the parameter must be large than zero");

            var list = new List<BinTree<U>>(array.Length);
            foreach (var e in array)
                list.Add(new BinTree<U>() { Node = new BinNode<U>() { Value = e } });

            for (int i = 1; i < array.Length; i++)
            {
                // as right child
                if (i % 2 == 0)
                    list[i / 2 - 1].RChild = list[i];
                // as left child
                else
                    list[i / 2].LChild = list[i];
            }
            return list[0];
        }
    }
    public class BinNode<T>
    {
        public T Value { get; set; }
        public bool Visited { get; set; }
    }

    public class Traversal<T>
    {
        public static void DFS(BinTree<T> tree)
        {
            Stack<BinTree<T>> stack = new Stack<BinTree<T>>();
            stack.Push(tree);

            BinTree<T> subTree = null;

            while (stack.Any())
            {
                subTree = stack.Pop();
                Console.Write(subTree.Node.Value + " ");

                if (subTree.RChild != null)
                    stack.Push(subTree.RChild);
                if (subTree.LChild != null)
                    stack.Push(subTree.LChild);
            }
        }

        public static void BFS(BinTree<T> tree)
        {
            Queue<BinTree<T>> queue = new Queue<BinTree<T>>();
            queue.Enqueue(tree);

            BinTree<T> subTree = null;

            while (!queue.Any())
            {
                subTree = queue.Dequeue();
                Console.Write(subTree.Node.Value + " ");

                if (subTree.LChild != null)
                    queue.Enqueue(subTree.LChild);
                if (subTree.RChild != null)
                    queue.Enqueue(subTree.RChild);
            }
        }
        public static void PrintLeftNodes(BinTree<T> tree)
        {
            Console.Write(tree.Node.Value);
            if (tree.LChild != null)
                PrintLeftNodes(tree.LChild);
            Console.Write('\n');
        }
        public static void PrintAsNormal(BinTree<T> tree)
        {
            var list = tree.ToList();
            Console.WriteLine("***************Print tree as list*******************");
            foreach (var e in list)
            {
                Console.Write(e.Value + " ");
            }
            Console.Write('\n');
            Console.WriteLine("----------------------------------------------------");

            var queue = tree.ToQueue();
            int depth = (int)Math.Log(queue.Count, 2) + 1;

            for (int i = 0; i < depth; i++)
            {
                // Preleading space count of each line
                int count1 = (int)Math.Pow(2, depth - i - 1) - 1;
                for (int j = 0; j < count1; j++)
                    Console.Write(' ');

                // Interval space count between two adjacent chars of each line
                int count2 = (int)Math.Pow(2, depth - i) - 1;
                int count3 = (int)Math.Pow(2, i);
                for (int j = 0; j < count3; j++)
                {
                    var node = queue.Dequeue();
                    Console.Write(node.Value);
                    for (int k = 0; k < count2; k++)
                        Console.Write(' ');
                }
                Console.Write('\n');
            }
        }
    }
}
