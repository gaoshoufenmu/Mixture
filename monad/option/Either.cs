using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace monad.option
{
    public class Either<L, R>
    {
        public L Left { get; private set; }
        public bool HasLeft { get; set; }
        public R Right { get; private set; }
        public bool HasRight { get; set; }

        public Either()
        {
            this.HasLeft = false;
            this.HasRight = false;
        }

        public Either<L, R> SetLeft(L left)
        {
            this.Left = left;
            this.HasLeft = true;
            return this;
        }
        public Either<L, R> SetRight(R right)
        {
            this.Right = right;
            this.HasRight = true;
            return this;
        }

        public static Either<L, R> FromLeft(L left)
        {
            return new Either<L, R>().SetLeft(left);
        }
        public static Either<L, R> FromRight(R right)
        {
            return new Either<L, R>().SetRight(right);
        }
        public override string ToString()
        {
            return this.HasRight ? this.Right.ToString() : this.Left.ToString();
        }

        
    }

    public class Result<T> : Either<string, T>
    {
        internal Result()
        { }

        public Result(T t)
        {
            this.SetRight(t);
        }

        public Result<T> SetError(string error)
        {
            this.SetLeft(error);
            return this;
        }

        public static Result<T> FromError(string error)
        {
            return new Result<T>().SetError(error);
        }

        public static void Test()
        {
            Console.WriteLine("5 * 6 * 7 is {0}",
                from x in new Result<int>(5)
                from y in new Result<int>(6)
                from z in new Result<int>(7)
                select x * y * z);

            Console.WriteLine("Error + 6 + 7 is {0}",
                from x in Result<int>.FromError("Error")
                from y in new Result<int>(6)
                from z in new Result<int>(7)
                select x + y + z);

            Console.WriteLine("5 + Error + 7 is {0}",
                from x in new Result<int>(5)
                from y in Result<int>.FromError("Error")
                from z in new Result<int>(7)
                select x + y + z);

            Console.WriteLine("5 + 6 + Error is {0}",
                from x in new Result<int>(5)
                from y in new Result<int>(6)
                from z in Result<int>.FromError("Error")
                select x + y + z);
        }
    }
}
