using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigData.ES
{
    public enum FieldType
    {
        _keyword_,
        _text_,
        _int_,
        _float_,
        _double_,
        _long_,
        _byte_,
        _short_,
        _date_,
        _bool_,
        _geo_
    }
    public class MapAttr : Attribute
    {
        public FieldType type;
        public string tokenizer;
        public string[] tokenizers;
    }
}
