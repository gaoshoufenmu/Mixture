using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace monad.option
{
    public static class Option
    {
        public static Option<A> Return<A>(A value) => Option<A>.Return(value);
    }

    public abstract class Option<A> : IEquatable<Option<A>>, IEquatable<A>
    {
        public abstract A ForceValue();

        // public abstract A Value { get; }
        public abstract bool Equals(Option<A> other);

        public abstract bool IsSome
        {
            [DebuggerStepThrough]
            get;
        }

        public static Option<A> None
        {
            get { return None<A>.Instance; }
        }

        [DebuggerStepThrough]
        public static Option<A> Return(A a)
        {
            return new Some<A>(a);
        }

        [DebuggerStepThrough]
        public abstract Option<B> Map<B>(Func<A, B> f);
        [DebuggerStepThrough]
        public abstract void Do(Action<A> callback);
        [DebuggerStepThrough]
        public abstract void Do(Action<A> valueCallback, Action nullCallback);

        [DebuggerStepThrough]
        public A Else(Func<A> callbackForNone)
        {
            if (this.IsSome)
                return this.ForceValue();
            return callbackForNone();
        }

        [DebuggerStepThrough]
        public A ElseDefault()
        {
            if (this.IsSome)
                return this.ForceValue();
            return default(A);
        }


        [DebuggerStepThrough]
        public Option<B> SelectMany<B>(Func<A, Option<B>> f)
        {
            return this.IsSome ? f(this.ForceValue()) : None<B>.Instance;
        }

        [DebuggerStepThrough]
        public Option<Tuple<A, B>> Concat<B>(Option<B> opt)
        {
            var none = None<Tuple<A, B>>.Instance;
            if (!this.IsSome)
                return none;

            if (!opt.IsSome)
                return none;

            return new Tuple<A, B>(this.ForceValue(), opt.ForceValue());
        }

        [DebuggerStepThrough]
        public Option<A> Where(Predicate<A> predicate)
        {
            if (this.IsSome && predicate(this.ForceValue()))
                return this;

            return None<A>.Instance;
        }

        [DebuggerStepThrough]
        public Option<A> Empty(Action nullCallback)
        {
            if (!this.IsSome)
                nullCallback();

            return this;
        }

        [DebuggerStepThrough]
        public Option<A> WhenSome(Action<A> callback)
        {
            if (this.IsSome)
                callback(this.ForceValue());

            return this;
        }

        [DebuggerStepThrough]
        public Option<A> WhenNone(Action callback)
        {
            if (!this.IsSome)
                callback();

            return this;
        }

        [DebuggerStepThrough]
        public Option<T> Cast<T>() where T : A
        {
            return SelectMany(a => a.MaybeCast<T>());
        }

        public static implicit operator Option<A>(A value)
        {
            if (typeof(A).IsByRef && Equals(null, value))
                return None;

            return Return(value);
        }

        public static bool operator true(Option<A> value)
        {
            return value.IsSome;
        }

        public static bool operator false(Option<A> value)
        {
            return !value.IsSome;
        }

        public static implicit operator bool(Option<A> value)
        {
            return value.IsSome;
        }

        public static bool operator !(Option<A> value)
        {
            return !value.IsSome;
        }

        public static Option<A> operator |(Option<A> left, Option<A> right)
        {
            if (left)
                return left;
            if (right)
                return right;

            return None;
        }

        public bool Equals(A other)
        {
            return IsSome && ForceValue().Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Option<A>)
                return Equals(obj as Option<A>);

            if (obj is A)
                return Equals((A)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return IsSome ? ForceValue().GetHashCode() : 0;
        }
    }


    [DebuggerDisplay("Some({value})")]
    public class Some<A> : Option<A>
    {
        private readonly A _value;
        public Some(A value)
        {
            this._value = value;
        }

        public override bool IsSome
        {
            [DebuggerStepThrough]
            get
            {
                return true;
            }
        }
        public override A ForceValue()
        {
            return _value;
        }
        //public override A Value { get { return _value; } }
        public override Option<B> Map<B>(Func<A, B> f)
        {
            return f(ForceValue());
        }

        public override void Do(Action<A> callback)
        {
            callback(_value);
        }
        public override void Do(Action<A> valueCallback, Action nullCallback)
        {
            valueCallback(_value);
        }

        public override bool Equals(Option<A> obj)
        {
            return obj.IsSome && ForceValue().Equals(obj.ForceValue());
        }
    }

    [DebuggerDisplay("None")]
    public class None<A> : Option<A>
    {
        private None()
        { }

        private static readonly None<A> _instance = new None<A>();
        public static None<A> Instance
        {
            get { return _instance; }
        }

        public override bool IsSome
        {
            [DebuggerStepThrough]
            get { return false; }
        }
        public override A ForceValue()
        {
            throw new InvalidOperationException("This does not have a value");
        }
        //public override A Value
        //{
        //    get { throw new InvalidOperationException(); }
        //}
        public override Option<B> Map<B>(Func<A, B> f)
        {
            return None<B>.Instance;
        }

        public override void Do(Action<A> callback)
        { }

        public override void Do(Action<A> valueCallback, Action nullCallback)
        {
            nullCallback();
        }

        public override bool Equals(Option<A> other)
        {
            return ReferenceEquals(this, other);
        }
    }


    public class Maybe<T>
    {
        public readonly static Maybe<T> Nothing = new Maybe<T>();
        public T Value { get; private set; }
        public bool HasValue { get; private set; }

        Maybe()
        {
            HasValue = false;
        }

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public override string ToString()
        {
            return HasValue ? Value.ToString() : "Nothing";
        }

        public static implicit operator Maybe<T>(T t)
        {
            return new Maybe<T>(t);
        }
    }
}
