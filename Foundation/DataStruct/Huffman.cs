using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foundation.datastructure
{
    public class HuffmanNode
    {
        public int weight;
        public char ch;
        public string code;
        public HuffmanNode left;
        public HuffmanNode right;
    }

    public class ListNode
    {
        public HuffmanNode node;
        public ListNode next;
    }

    public class QueueH
    {
        ListNode front;
        ListNode rear;

        public QueueH()
        {
            front = rear = new ListNode();
        }

        public int EnQueue(HuffmanNode node)
        {
            var lNode = new ListNode() { node = node };
            rear.next = lNode;
            rear = lNode;
            return 0;
        }

        public int EnQueueByOrder(HuffmanNode node)
        {
            var n = front.next;
            var f = front;
            while(n != null)
            {
                if (n.node.weight < node.weight)
                {
                    n = n.next;
                    f = n.next;
                }
                else
                    break;
            }

            if(n != null)
            {
                var temp = new ListNode() { node = node };
                f.next = temp;
                rear = temp;
                return 0;
            }

            var temp1 = new ListNode() { node = node };
            f.next = temp1;
            temp1.next = n;
            return 0;
        }

        public bool _IsEmpty() => front.next.next != null;

        public bool IsEmpty() => front == rear;

        public HuffmanNode DeQueue()
        {
            var temp = front;
            front = temp.next;
            temp = null;
            return front.node;
        }

        public HuffmanNode CreateHuffmanTree()
        {
            while(!_IsEmpty())
            {
                var left = DeQueue();
                var right = DeQueue();
                var cur = new HuffmanNode() { weight = left.weight + right.weight, left = left, right = right };
                EnQueueByOrder(cur);
            }
            return front.next.node;
        }

        public int CodeHuffman(HuffmanNode root)
        {
            EnQueue(root);

            while(!IsEmpty())
            {
                var cur = DeQueue();
                if (cur.left == null && cur.right == null)
                    Console.WriteLine($"{cur.ch}, {cur.weight}, {cur.code}");
                else
                {
                    cur.left.code = cur.code + "0";
                    EnQueue(cur.left);

                    cur.right.code = cur.code;
                    EnQueue(cur.right);
                }
            }
            return 0;
        }

        public void DecodeHuffman(HuffmanNode root, char[] codes)
        {
            var tree = root;
            for(int i = 0; i < codes.Length; i++)
            {
                var c = codes[i];
                if(c == '0')
                {
                    tree = tree.left;
                    if(tree.left == null)
                    {
                        Console.WriteLine(tree.ch);
                        tree = root;
                    }
                }
                else if(c == '1')
                {
                    tree = tree.right;
                    if(tree.left == null)
                    {
                        Console.WriteLine(tree.ch);
                        tree = root;
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

    
}
