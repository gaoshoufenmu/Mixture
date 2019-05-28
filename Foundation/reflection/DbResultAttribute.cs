using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foundation.reflection
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class DbResultAttribute : Attribute
    {
        public bool DefaultAsColumn { get; private set; }
        public DbResultAttribute() : this(true)
        { }

        public DbResultAttribute(bool defaultAsDbColumn)
        {
            DefaultAsColumn = defaultAsDbColumn;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    internal sealed class DbColumnAttribute : Attribute
    {
        public DbColumnAttribute()
        { }
        public string ColumnName { get; set; }
        public bool Ignore { get; set; }
    }

    [DbResult]
    public class MyClass
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }

        [DbColumn(ColumnName = "DecimalValue")]
        public decimal? NullableDecimalAndDifferentName { get; set; }
        public MyEnum EnumIsAlsoSupportted { get; set; }
        public MyEnum? NullableEnumIsAlsoSupportted { get; set; }
        [DbColumn(Ignore = true)]
        public object NonDbValue { get; set; }
    }

    public enum MyEnum
    {
        x,
        y,
        z
    }
}
