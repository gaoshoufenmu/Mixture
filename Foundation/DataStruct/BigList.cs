using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.DataStruct
{
    /// <summary>
    /// 大型列表，用于存储大容量数据，上限为0x1000000
    /// 非顺序存储，保证删除的效率高
    /// </summary>
    public class BigList<T> : IDisposable, IEnumerable<T>
    {
        private const int size_s = 0x100;
        private const int size_m = 0x100;
        private const int size_l = 0x100;
        /* 结构：
         * 1. 小号，T[]，大小固定为0x100
         * 2. 中号，List<T[]>，包含不超过0x100数量的小号
         * 3. 大号，List<List<T[]>>，包含不超过0x100数量的中号
         * */
        private List<List<T[]>> _large;
        private int _count;
        /// <summary>
        /// 列表中项数量
        /// </summary>
        private int Count => _count;
        /// <summary>
        /// 空闲的index
        /// </summary>
        private Queue<int> _idleIndice;
        public BigList(int capacity = 0)
        {
            if(capacity > 0)
            {
                var count_m = capacity >> 16;       // 中号数量
                if (count_m == 0) count_m = 1;
                _large = new List<List<T[]>>(count_m);
            }
            else
                _large = new List<List<T[]>>();
        }

        /// <summary>
        /// 添加给定项，返回项的index
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int Add(T t)
        {
            int index = 0;
            if (_idleIndice == null || _idleIndice.Count == 0)
                index = _count;
            else
                index = _idleIndice.Dequeue();

            var idx_m = index >> 16;        // 中号index：落在哪个中号上
            if (idx_m == size_l) throw new Exception("BigList elements are full");

            var idx_s = (index & 0xffff) >> 8;     // 小号index：落在哪个小号上
            var idx = index & 0xff;         // 在小号中的index

            if (idx_m == _large.Count)
                _large.Add(new List<T[]>());
            var medium = _large[idx_m];

            if (idx_s == medium.Count)
                medium.Add(new T[size_s]);
            var small = medium[idx_s];
            small[idx] = t;
            _count++;
            return index;
        }

        public T Get(int index)
        {
            if (index < 0) throw new Exception("index is out of range");

            var idx_m = index >> 16;        // 中号index：落在哪个中号上
            if (idx_m == size_l) throw new Exception("index is out of range");

            if (idx_m >= _large.Count) throw new Exception("index is out of range");

            var medium = _large[idx_m];
            var idx_s = (index & 0xffff) >> 8;     // 小号index：落在哪个小号上
            if (idx_s >= medium.Count) throw new Exception("index is out of range");

            var small = medium[idx_s];
            return small[index & 0xff];
        }

        public void Set(int index, T t)
        {
            if (index < 0) throw new Exception("index is out of range");

            var idx_m = index >> 16;        // 中号index：落在哪个中号上
            if (idx_m == size_l) throw new Exception("index is out of range");

            if (idx_m >= _large.Count) throw new Exception("index is out of range");

            var medium = _large[idx_m];
            var idx_s = (index & 0xffff) >> 8;     // 小号index：落在哪个小号上
            if (idx_s >= medium.Count) throw new Exception("index is out of range");

            var small = medium[idx_s];
            small[index & 0xff] = t;
        }

        public T this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }


        /// <summary>
        /// 根据指定index删除项
        /// </summary>
        /// <param name="index"></param>
        private void Remove(int index)
        {
            if (index < 0) throw new Exception("index is out of range");

            var idx_m = index >> 16;        // 中号index：落在哪个中号上
            if (idx_m == size_l) throw new Exception("index is out of range");

            if (idx_m >= _large.Count) throw new Exception("index is out of range");

            var medium = _large[idx_m];
            var idx_s = (index & 0xffff) >> 8;     // 小号index：落在哪个小号上
            if (idx_s >= medium.Count) throw new Exception("index is out of range");

            var small = medium[idx_s];
            var idx = index & 0xff;         // 在小号中的index

            _count--;
            small[idx] = default(T);
            if (_idleIndice == null)
                _idleIndice = new Queue<int>();
            _idleIndice.Enqueue(index);
        }

        public void Dispose()
        {
            for (int i = 0; i < _large.Count; i++)
            {
                for (int j = 0; j < _large[i].Count; j++)
                    _large[i][j] = null;

                _large[i] = null;
            }
            _large = null;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private BigList<T> _list;
            private int index;
            private T current;
            public T Current => current;

            object IEnumerator.Current => current;

            public Enumerator(BigList<T> bl)
            {
                index = 0;
                current = default(T);
                _list = bl;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                current = _list.Get(index);
                index++;
                return true;
            }

            public void Reset()
            {
                index = 0;
                current = default(T);
            }
        }
    }
}
