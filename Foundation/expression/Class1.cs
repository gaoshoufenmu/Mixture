using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace foundation.expression
{
    public class VisitExpression
    {
        public string SerializeExpression(Expression expression)
        {
            return expression.ToString();
        }
        public static Expression GetExpression1()
        {
            Expression<Func<A, string>> expr = a => a.Name;
            return expr;
        }
        public static Expression GetExpression2()
        {
            Expression<Func<A, int>> expr2 = a => a.Score;
            return expr2;
        }
        public static string Assign()
        {
            Expression<Func<A, string>> expr = i => i.Name;
            var a = new A();
            a.Name = "name";
            return expr.Compile().Invoke(a);
        }
        public List<string> list { get; set; }
        public static Expression Compare(string name)
        {
            Expression<Func<A, bool>> expr = a => a.Name == "";
            return expr;
        }
    }
    public class A
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    
    
}
