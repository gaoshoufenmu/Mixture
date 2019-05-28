using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Foundation.IO
{
    public class ExpVisitorSql : ExpressionVisitor
    {
        private Stack<string> _stack = new Stack<string>();
        public MemberInfo Member { get; private set; }
        public MemberInfo Resolve(Expression exp)
        {
            if (Member == null)
                Visit(exp);

            return Member;
        }
    }
}
