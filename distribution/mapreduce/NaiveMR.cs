/**
 * Local machine map reduce with parallel execution
 * 
 * */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace distribution.mapreduce
{
    public class NaiveMR
    {
        public static int NumberOfCores = 8;

        public static Dictionary<T3, List<T4>> Execute<T1, T2, T3, T4>(Func<T1, T2, List<KeyValuePair<T3, T4>>> map, Func<T3, List<T4>, List<T4>> reduce, 
            Dictionary<T1, T2> input)
        {
            var results = new Dictionary<T3, List<T4>>();
            var maps = new Dictionary<T3, List<T4>>();

            // divides input into NumberOfCores' parts. For each element of each part, map it into List<KeyValuePair<T3, T4>, 
            // and select Key T3 and Value T4, and add them two into maps.
            // pair -> KeyValuePair<T1, T2>, i -> KeyValuePair<T3, T4>
            input.Partition(NumberOfCores, l => l.ForEach(pair => maps.Add(map(pair.Key, pair.Value), i => i.Key, i => i.Value)));

            // divides maps into NumberOfCores' parts. For each map of each part, reduce it into List<T4>, and select Key T3 and Value T4, and add them two into results.
            // ma -> KeyValuePair<T3, List<T4>>, i -> T4
            maps.Partition(NumberOfCores, m => m.ForEach(ma => results.Add(reduce(ma.Key, ma.Value), i => ma.Key, i => i)));

            return results;
        }

        public static Dictionary<T2, T3> Execute<T1, T2, T3>(Func<T1, List<Tuple<T2, T3>>> map, Func<T2, List<T3>, T3> reduce, List<T1> list)
        {
            var maps = new Dictionary<T2, List<T3>>();
            var results = new Dictionary<T2, T3>();

            list.Partition(NumberOfCores, l => l.ForEach(t1 => maps.Add(map(t1), t => t.Item1, t => t.Item2)));
            maps.Partition(NumberOfCores, m => m.ForEach(ma => results.Add(reduce(ma.Key, ma.Value), i => ma.Key, i => i)));
            return results;
        }

        #region demo
        public static Dictionary<string, int> StatKeywords() =>
            Execute(Map, Reduce, Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/resource/").Select(f => File.ReadAllText(f)).ToList());

        public static List<Tuple<string, int>> Map(string text) =>
            text.Split('\n', ' ', '.', ',', '\r').Select(w => new Tuple<string, int>(w, 1)).ToList();

        public static int Reduce(string word, List<int> counts) => counts.Sum();
        #endregion
    }


}
