using System;

namespace TypeUtil
{
    public struct Sum<T1, T2>
    {

        private readonly object val;
        private readonly byte injection;

        private Sum(object val, byte injection)
        {
            this.val = val;
            this.injection = injection;
        }

        public T Match<T>(Func<T1, T> l, Func<T2, T> r)
        {
            switch (injection)
            {
                case 0:
                    return l((T1) val);
                case 1:
                    return r((T2) val);
            }
            throw new Exception();
        }
        
        
        
        public static Sum<T1, T2> Inr(T2 value)
        {
            return new Sum<T1,T2>(value,1);
        }

        public static Sum<T1, T2> Inl(T1 value)
        {
            return new Sum<T1, T2>(value,0);
        }


        public override string ToString()
        {
            return Match(l =>"inl of " + l.ToString(), r =>"inr of " + r.ToString());
        }
    }
    public struct Sum<T1, T2, T3>
    {
        //public abstract T Match<T>(Func<T1, T> l, Func<T2, T> r);

        private readonly object val;
        private readonly byte injection;

        private Sum(object val, byte injection)
        {
            this.val = val;
            this.injection = injection;
        }

        public T Match<T>(Func<T1, T> f0, Func<T2, T> f1, Func<T3,T> f2)
        {
            switch (injection)
            {
                case 0:
                    return f0((T1) val);
                case 1:
                    return f1((T2) val);
                case 2:
                    return f2((T3) val);
            }

            throw new Exception();
        }
        
        
        
        public static Sum<T1, T2,T3> In1(T2 value)
        {
            return new Sum<T1,T2,T3>(value,1);
        }

        public static Sum<T1, T2, T3> In0(T1 value)
        {
            return new Sum<T1, T2,T3>(value,0);
        }

        public static Sum<T1, T2, T3> In2(T2 value)
        {
            return new Sum<T1, T2, T3>(value,2);
        }

        public override string ToString()
        {
            return Match(x =>"in0 of " + x.ToString(), x =>"in1 of " + x.ToString(),x => "In2 of " + x.ToString());
        }
    }


}