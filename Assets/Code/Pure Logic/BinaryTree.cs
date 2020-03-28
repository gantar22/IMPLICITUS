using System;

namespace Lambda
{
    public class BinaryTree<T>
    {
        private object val;
        private byte injection;

        private BinaryTree(object val, byte injection)
        {
            this.val = val;
            this.injection = injection;
        }

        public static BinaryTree<T> Leaf(T t)
        {
            return new BinaryTree<T>(t, 0);
        }

        public static BinaryTree<T> Node(BinaryTree<T> l, BinaryTree<T> r)
        {
            return new BinaryTree<T>(Tuple.Create(l,r),1);
        }

        public R Match<R>(Func<BinaryTree<T>, BinaryTree<T>, R> Recur, Func<T, R> BC)
        {
            switch (injection)
            {
                case 0:
                    return BC((T)val);
                case 1:
                    var (l,r) = (Tuple<BinaryTree<T>,BinaryTree<T>>)val;
                    return Recur(l, r);
                default:
                    throw new Exception("bad binary tree");
            }
        }

        public int LeftDepth()
        {
            return Match((l,r) => l.LeftDepth() + 1,x => 0);
        }
    }
}