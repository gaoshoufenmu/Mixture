using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.DataStruct
{
    /// <summary>
    /// 大型字典
    /// 没有实现实时均衡存储
    /// </summary>
    public class BigDict<K, V> : IDictionary<K, V>
    {
        internal List<IDictionary<K, V>> _dicts;

        private int _count;
        public V this[K key]
        {
            get
            {
                V v;
                if (TryGetValue(key, out v))
                    return v;
                throw new KeyNotFoundException();
            }
            set => Put(key, value);
        }

        public void Put(K k, V v)
        {
            var hashCode = k.GetHashCode();
            var idx_s = hashCode & 0xfff;

            if (idx_s >= _dicts.Count)
            {
                for (int i = _dicts.Count; i < idx_s + 1; i++)
                    _dicts.Add(new Dictionary<K, V>());
            }
            _dicts[idx_s][k] = v;
            if (!_dicts[idx_s].ContainsKey(k))
                _count++;
            
        }

        public ICollection<K> Keys => throw new NotImplementedException();

        public ICollection<V> Values => throw new NotImplementedException();

        public int Count => _count;

        public bool IsReadOnly => false;

        public BigDict(int capacity)
        {
            if (capacity > 0)
            {
                var count_s = capacity >> 12;
                if (count_s == 0) count_s = 1;
                _dicts = new List<IDictionary<K, V>>(count_s);
            }
            else
                _dicts = new List<IDictionary<K, V>>();
        }

        public void Add(K key, V value)
        {
            var hashCode = key.GetHashCode();
            var idx_s = hashCode & 0xfff;

            if (idx_s >= _dicts.Count)
            {
                for (int i = _dicts.Count; i < idx_s + 1; i++)
                    _dicts.Add(new Dictionary<K, V>());
            }
            _dicts[idx_s].Add(key, value);
            _count++;
        }

        public void Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            for (int i = 0; i < _dicts.Count; i++)
                _dicts[i].Clear();
        }

        public bool Contains(KeyValuePair<K, V> item) => throw new NotImplementedException();

        public bool ContainsKey(K key)
        {
            var hashCode = key.GetHashCode();
            var idx_s = hashCode & 0xfff;
            if (idx_s >= _dicts.Count) return false;

            return _dicts[idx_s].ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => new Enumerator(this);

        public bool Remove(K key)
        {
            var hashCode = key.GetHashCode();
            var idx_s = hashCode & 0xfff;

            if (idx_s >= _dicts.Count) return false;

            return _dicts[idx_s].Remove(key);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(K key, out V value)
        {
            var hashCode = key.GetHashCode();
            var idx_s = hashCode & 0xfff;
            if (idx_s >= _dicts.Count)
            {
                value = default(V);
                return false;
            }
            if (_dicts[idx_s].TryGetValue(key, out value))
                return true;

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);


        public class Enumerator : IEnumerator<KeyValuePair<K, V>>
        {
            public KeyValuePair<K, V> Current
            {
                get
                {
                    var dict = _bd._dicts[_index_1];
                    var key = _keys[_index_2];
                    return new KeyValuePair<K, V>(key, dict[key]);
                }
            }

            object IEnumerator.Current => throw new NotImplementedException();

            private int _index_1;       // 一级索引
            private int _index_2;       // 二级索引
            private K[] _keys;
            private BigDict<K, V> _bd;
            public Enumerator(BigDict<K, V> bd)
            {
                _bd = bd;
                _index_1 = 0;
                _index_2 = -1;
            }

            public void Dispose()
            {
                _index_1 = _index_2 = -1;
                _bd = null;
            }

            public bool MoveNext()
            {
                if (_bd == null) return false;
                
                while(_index_1 < _bd._dicts.Count)
                {
                    _index_2++;
                    while (_index_2 < _keys.Length)
                        return true;

                    _index_2 = -1;
                    _index_1++;
                    if (_index_1 < _bd._dicts.Count)
                        _keys = _bd._dicts[_index_1].Keys.ToArray();
                }
                return false;
            }

            public void Reset()
            {
                _index_1 = 0;
                _index_2 = -1;
            }
        }
    }
}
