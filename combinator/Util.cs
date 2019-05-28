using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combinator.local
{
    public static class Util
    {
        public static ICoder<T> WithPostfix<T>(this ICoder<T> coder, string postfix)
        {
            var coderPostfix = new UnitCoder<T>(postfix);

            return new SequenceCoder<T>(new ICoder[] { coder, coderPostfix }, (meta, arr) => string.Join("", coder.Code(meta), coderPostfix.Code(meta)));
        }
        public static ICoder<T> WithPrefix<T>(this ICoder<T> coder, string prefix)
        {
            var coderPrefix = new UnitCoder<T>(prefix);
            return new SequenceCoder<T>(new ICoder[] { coderPrefix, coder }, (meta, arr) => string.Join("", coderPrefix.Code(meta), coder.Code(meta)));
        }
        public static ICoder<T> Brace<T>(this ICoder<T> coder)
        {
            return coder.WithPostfix("}").WithPrefix("{");
        }

        public static ICoder<IEnumerable<T>> Many<T>(this ICoder<T> coder, string seperator) where T : class
        {
            return new RepeatedCoder<T>(coder, seperator, _ => true);
        }

        public static ICoder<T> Combine_Gen<T, T1>(ICoder<T> tCoder, ICoder<T1> t1Coder, Func<T, T1> selector)
        {
            return new SequenceCoder<T>(new ICoder[] { tCoder, t1Coder }, (meta, arr) => $"{tCoder.Code(meta)}-{t1Coder.Code(selector(meta))}");
        }
        public static BasicCoder<Meta> Default_BasicCoder { get { return new BasicCoder<Meta>(t => $"{t.Type}-{t.Name}-{t.Value}"); } }

        public static ICoder<Meta> Semicolon_Coder_Get() => Default_BasicCoder.WithPostfix(";");

        public static ICoder<Meta> Brace_Coder_Get() => Default_BasicCoder.Brace();
        public static ICoder<IEnumerable<Meta>> Repeater => Default_BasicCoder.WithPostfix(";").Many("\n");
    }

    public class Meta
    {
        public string Type;
        public string Name;
        public string Value;
    }
}
