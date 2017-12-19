using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.ClassicAlgos
{
    /// <summary>
    /// 排序
    /// </summary>
    public class Sort
    {
        #region Bubble
        /// <summary>
        /// 冒泡排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        public static void BubbleSort<V>(V[] array, int asc = 1) where V : IComparable<V>
        {
            for(int i = 0; i < array.Length; i++)
            {
                for(int j = i+1; j < array.Length; j++)
                {
                    if(array[i].CompareTo(array[j]) * asc > 0)
                    {
                        Swap(array, i, j);
                    }
                }
            }
        }
        #endregion

        #region Select
        /// <summary>
        /// 选择排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        /// <param name="n">指示排序出top n 的数据，默认为0表示所有数据都进行排序</param>
        public static void SelectSort<V>(V[] array, int asc = 1, int n = 0) where V : IComparable<V>
        {
            if (n == 0) n = array.Length;

            for(int i = 0; i < array.Length; i++)
            {
                int desIndex = i;
                for(int j = i+1; j < array.Length; j++)
                {
                    if (array[j].CompareTo(array[desIndex]) * asc < 0)
                        desIndex = j;
                }
                if(desIndex != i)
                {
                    Swap(array, i, desIndex);
                }
                if (i + 1 >= n) break;      // top n 的数据已经排好序，则退出
            }
        }
        #endregion

        #region Insert
        /// <summary>
        /// 插入排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        public static void InsertSort<V>(V[] array, int asc = 1) where V : IComparable<V>
        {
            for(int i = 1; i < array.Length; i++)
            {
                if(array[i - 1].CompareTo(array[i]) * asc > 0)
                {
                    var temp = array[i];
                    int j = i;
                    while(j > 0 && array[j - 1].CompareTo(temp)*asc > 0)
                    {
                        array[j] = array[j - 1];
                        j--;
                    }
                    array[j] = temp;
                }
            }
        }
        #endregion

        #region Quick
        /// <summary>
        /// 快速排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        /// <param name="n">排序得到最大（小）的n个数即可，默认值为0，表示全排序</param>
        /// <remarks>当n取值为(0,array.Length)时，仅保证右边起n个数比左边array.Length-n个数大（小），但不保证这n个数内部的排序</remarks>
        public static void QuickSort<V>(V[] array, int asc = 1, int n = 0) where V : IComparable<V> => QuickSort(array, asc, 0, array.Length - 1, n);

        private static void QuickSort<V>(V[] array, int asc, int left, int right, int n) where V : IComparable<V>
        {
            if(left < right)
            {
                var middle = PartitionSort(array, asc, left, right);
                if (n != 0 && array.Length - middle + 1 >= n) return;       //! 仅保证右边起 n 个数是数组中最大（小）的n个数，但不保证这n个数内部排序，注意与选择排序的top n 的区别
                QuickSort(array, asc, left, middle - 1, n);
                QuickSort(array, asc, middle + 1, right, n);
            }
        }
        private static int PartitionSort<V>(V[] array, int asc, int start, int end) where V : IComparable<V>
        {
            var pivot = array[start];
            while(start < end)
            {
                while (start < end && array[end].CompareTo(pivot) * asc >= 0) end--;
                if (start < end)
                    array[start++] = array[end];
                while (start < end && array[start].CompareTo(pivot) * asc <= 0) start++;
                if (start < end)
                    array[end--] = array[start];
            }
            array[start] = pivot;
            return start;
        }
        #endregion

        #region Gnome
        /// <summary>
        /// 地精排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        /// <param name="n">指示排序出top n 的数据，默认为0表示所有数据都进行排序</param>
        public static void GnomeSort<V>(V[] array, int asc = 1, int n = 0) where V : IComparable<V>
        {
            if (n == 0) n = array.Length;
            int i = 0;
            while(i < array.Length)
            {
                if (i == 0 || array[i - 1].CompareTo(array[i]) * asc <= 0)
                {
                    i++;
                    if (i >= n) return;              // top n data is sorted, exit;
                }
                else
                {
                    Swap(array, i - 1, i);
                    i--;
                }
            }
        }
        #endregion

        #region Merge
        /// <summary>
        /// 归并排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        public static void MergeSort<V>(V[] array, int asc) where V : IComparable<V> => MergeSort(array, asc, 0, array.Length, new V[array.Length]);

        private static void MergeSort<V>(V[] arrA, int asc, int first, int last, V[] arrB) where V : IComparable<V>
        {
            if(first + 1 < last)
            {
                var middle = (first + last) / 2;
                MergeSort(arrA, asc, first, middle, arrB);
                MergeSort(arrA, asc, middle, last, arrB);
                MergeSort(arrA, asc, first, middle, last, arrB);
            }
        }
        private static void MergeSort<V>(V[] arrA, int asc, int first, int middle, int last, V[] arrB) where V : IComparable<V>
        {
            int i = first, j = middle;
            int k = 0;

            while(i < middle && j < last)
            {
                if (arrA[i].CompareTo(arrA[j]) * asc < 0)
                    arrB[k++] = arrA[i++];
                else
                    arrB[k++] = arrA[j++];
            }
            while (i < middle)
                arrB[k++] = arrA[i++];
            while (j < last)
                arrB[k++] = arrA[j++];

            for (int v = 0; v < k; v++)
                arrA[first + v] = arrB[v];
        }

        #endregion

        #region Heap
        /// <summary>
        /// 堆排序
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：升序；-1：降序</param>
        public static void HeapSort<V>(V[] array, int asc = 1) where V : IComparable<V>
        {
            for (int i = array.Length / 2 - 1; i >= 0; i--)
                ExtreHeap_Rec(array, asc, i, array.Length);     // can be substituted by ExtreHeap_Iter

            for (int i = array.Length - 1; i >= 1; i--)
            {
                Swap(array, 0, i);
                ExtreHeap_Rec(array, asc, 0, i);                // can be substituted by ExtreHeap_Iter
            }
        }

        /// <summary>
        /// 循环方式生成极值堆
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：最大值堆；-1：最小值堆</param>
        /// <param name="i"></param>
        /// <param name="size"></param>
        private static void ExtreHeap_Iter<V>(V[] array, int asc, int i, int size) where V : IComparable<V>
        {
            // create a full binary tree. parent index is i, then
            int l_child = 2 * i + 1;    // left child index
            int r_child = 2 * i + 2;    // right child index
            int extre = i;

            while(l_child < size)
            {
                if (asc * array[l_child].CompareTo(array[extre]) > 0)
                    extre = l_child;
                if (r_child < size && array[r_child].CompareTo(array[extre]) * asc > 0)
                    extre = r_child;

                if (extre != i)
                {
                    Swap(array, i, extre);

                    i = extre;
                    l_child = 2 * i + 1;
                    r_child = 2 * i + 2;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// 递归方式生成极值堆
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="array"></param>
        /// <param name="asc">1：最大值堆；-1：最小值堆</param>
        /// <param name="i"></param>
        /// <param name="size"></param>
        private static void ExtreHeap_Rec<V>(V[] array, int asc, int i, int size) where V : IComparable<V>
        {
            // create a full binary tree. parent index is i, then
            int l_child = 2 * i + 1;    // left child index
            int r_child = 2 * i + 2;    // right child index

            int extre = i;
            if (l_child < size && array[l_child].CompareTo(array[extre]) * asc > 0)
                extre = l_child;
            if (r_child < size && array[r_child].CompareTo(array[extre]) * asc > 0)
                extre = r_child;

            if(extre != i)
            {
                Swap(array, i, extre);
                ExtreHeap_Rec(array, asc, extre, size);
            }
        }
        #endregion

        private static void Swap<V>(V[] array, int i, int j)
        {
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
