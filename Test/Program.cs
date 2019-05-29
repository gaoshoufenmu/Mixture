using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = StatisticCS.Data.Create(AppDomain.CurrentDomain.BaseDirectory + "data.txt");
        }


    }
}
