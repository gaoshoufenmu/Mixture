using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combinator.local
{
    public interface ICoder
    {
        string Code(object meta);
    }

    public interface ICoder<in T> : ICoder
    {
        string Code(T meta);
    }

    public abstract class CoderBase<T> : ICoder<T>
    {
        private readonly T instance;
        public abstract string Code(T meta);
        public string Code(object meta)
        {
            if (meta is T)
                return Code((T)meta);

            throw new Exception("type error...");
        }
    }

    public class SequenceCoder<T> : CoderBase<T>
    {
        readonly ICoder[] coderArr;
        readonly Func<T, ICoder[], string> coder_Joiner;

        public SequenceCoder(ICoder[] coderArr, Func<T, ICoder[], string> coder_Joiner)
        {
            this.coderArr = coderArr;
            this.coder_Joiner = coder_Joiner;
        }

        public override string Code(T meta)
        {
            return coder_Joiner(meta, coderArr);
        }
    }


    public class UnitCoder<T> : ICoder<T>
    {
        readonly string output;
        public UnitCoder(string output)
        {
            this.output = output;
        }

        public string Code(object meta)
        {
            throw new NotImplementedException();
        }

        public string Code(T meta)
        {
            return output;
        }
    }

    public class ZeroCoder<T> : ICoder<T>
    {
        private static ZeroCoder<T> instance;
        public static ZeroCoder<T> Instance
        {
            get { return instance ?? (instance = new ZeroCoder<T>()); }
        }

        private ZeroCoder()
        { }

        public string Code(T meta)
        {
            return string.Empty;
        }

        public string Code(object meta)
        {
            throw new NotImplementedException();
        }
    }

    public class BasicCoder<T> : ICoder<T>
    {
        private readonly Func<T, string> func;
        public BasicCoder(Func<T, string> func)
        {
            this.func = func;
        }

        public string Code(object meta)
        {
            throw new NotImplementedException();
        }

        public string Code(T meta)
        {
            return func(meta);
        }


    }

    public class RepeatedCoder<T> : CoderBase<IEnumerable<T>>
    {
        private readonly ICoder coder;
        private readonly string seperator;
        private readonly Func<T, bool> predicate;
        public RepeatedCoder(ICoder<T> coder, string seperator, Func<T, bool> predicate)
        {
            this.coder = coder;
            this.seperator = seperator;
            this.predicate = predicate;
        }

        public override string Code(IEnumerable<T> meta)
        {
            bool first = true;
            return meta.Where(m => predicate(m)).Select(m => coder.Code(m)).Aggregate("", (val, cur) =>
            {
                if (first)
                {
                    first = false;
                    return val + cur;
                }
                return val + seperator + cur;
            });
        }
    }
}
