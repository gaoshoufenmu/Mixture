using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using distribution.mapreduce;

namespace UnitTest
{
    [TestClass]
    public class NaiveMRTest
    {
        [TestMethod]
        public void TestStat()
        {
            var keywords = new[] { "et", "Cicero", "iam" };
            var output = NaiveMR.StatKeywords();
            foreach (var w in keywords)
            {
                if (output.ContainsKey(w))
                    Console.WriteLine(output[w]);
                else
                    Console.WriteLine("nothing");
            }
        }
    }
}
