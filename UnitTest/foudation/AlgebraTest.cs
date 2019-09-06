using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using foundation.algorithm;

namespace UnitTest.foudation
{
    [TestClass]
    public class AlgebraTest
    {
        [TestMethod]
        public void TestPrimeFactors()
        {
            var factors = Algebra.PrimeFactors(600851475143);
            foreach (var f in factors)
                Console.WriteLine(f);
        }
    }
}
