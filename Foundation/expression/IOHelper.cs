using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

namespace foundation.expression
{
    public class IOHelper
    {
        static TextReader In = Console.In;
        static char? lastChar = null;

        static void Main(string[] args)
        {
            if (args.Length != 1 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Usage: {0} <inputfile>", Assembly.GetEntryAssembly().GetName().Name);
                return;
            }

            string file = args[0];
            if (!File.Exists(file))
            {
                Console.WriteLine("File not found: {0}", file);
                return;
            }

            try
            {
                Execute(Parse(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static Expression1 Parse(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            using (StreamReader sr = new StreamReader(fs))
                return Parse(sr);
        }

        static void Execute(Expression1 ex)
        {
            Continuation exit = f => { Environment.Exit(0); return null; };
            for (Task task = ex.Eval(exit); true; task = task())
                ;
        }

        static Function S = (x, c) => c((y, c1) => c1((z, c2) =>
        {
            Expression1 arg = new FunctionExpression(z);
            return () => new ApplyExpression(
                new ApplyExpression(
                    new FunctionExpression(x),
                    arg),
                new ApplyExpression(
                    new FunctionExpression(y),
                    arg)
                ).Eval(c2);
        }));
        static Function K = (x, c) => c((y, c1) => c1(x));
        static Function I = (x, c) => c(x);
        static Function V = (x, c) => c(V);
        static Function D = (f, c) => c(D1(new FunctionExpression(f)));
        static Function CallCC = (x, c) => () => x((f, c2) => c(f), c);
        static Function Exit = (x, c) => () => { Environment.Exit(0); return null; };
        static Function Read = (x, c) =>
        {
            int ch = In.Read();
            lastChar = ch == -1 ? null : (char?)ch;
            return () => x(lastChar != null ? I : V, c);
        };
        static FC<char> Print = ch => (x, c) => { Console.Write(ch); return c(x); };
        static Function Reprint = (x, c) => () => x(lastChar != null ? Print(lastChar.Value) : V, c);
        static FC<char> Compare = ch => (x, c) => () => x(lastChar == ch ? I : V, c);
        internal static FC<Expression1> D1 = ex => (f1, c1) => () => ex.Eval(f2 => () => f2(f1, c1));

        static Expression1 Parse(StreamReader sr)
        {
            char ch;
            do
            {
                ch = ReadNextChar(sr);
                if (ch == '#')
                {
                    sr.ReadLine();
                    ch = '\n';
                }
            } while (char.IsWhiteSpace(ch));

            switch (ch)
            {
                case '`':
                    return new ApplyExpression(Parse(sr), Parse(sr));
                case 'S':
                case 's':
                    return new FunctionExpression(S);
                case 'K':
                case 'k':
                    return new FunctionExpression(K);
                case 'I':
                case 'i':
                    return new FunctionExpression(I);
                case 'V':
                case 'v':
                    return new FunctionExpression(V);
                case 'D':
                case 'd':
                    return new DelayedFunctionExpression(D);
                case 'C':
                case 'c':
                    return new FunctionExpression(CallCC);
                case 'E':
                case 'e':
                    return new FunctionExpression(Exit);
                case 'R':
                case 'r':
                    return new FunctionExpression(Print('\n'));
                case '@':
                    return new FunctionExpression(Read);
                case '.':
                    return new FunctionExpression(Print(ReadNextChar(sr)));
                case '|':
                    return new FunctionExpression(Reprint);
                case '?':
                    return new FunctionExpression(Compare(ReadNextChar(sr)));
                default:
                    throw new Exception("Unrecognized input symbol: " + ch);
            }
        }

        static char ReadNextChar(StreamReader sr)
        {
            int c = sr.Read();
            if (c == -1)
                throw new Exception("Unexpected end of file.");
            return (char)c;
        }
    }

    abstract class Expression1
    {
        public abstract Task Eval(Continuation c);
    }

    class ApplyExpression : Expression1
    {
        internal ApplyExpression(Expression1 f, Expression1 arg)
        {
            Operator = f;
            Operand = arg;
        }

        public Expression1 Operator { get; private set; }
        public Expression1 Operand { get; private set; }

        public override Task Eval(Continuation c)
        {
            return () => Operator.Eval(f => () => Operator is DelayedFunctionExpression ? c(IOHelper.D1(Operand)) : Operand.Eval(arg => () => f(arg, c)));
        }
    }

    class FunctionExpression : Expression1
    {
        internal FunctionExpression(Function f)
        {
            Function = f;
        }

        public Function Function { get; private set; }

        public override Task Eval(Continuation cont)
        {
            return cont(Function);
        }
    }

    class DelayedFunctionExpression : FunctionExpression
    {
        internal DelayedFunctionExpression(Function f) : base(f) { }
    }

    delegate Task Task();
    delegate Task Function(Function arg, Continuation c);
    delegate Task Continuation(Function f);
    delegate Function FC<T>(T t);

}