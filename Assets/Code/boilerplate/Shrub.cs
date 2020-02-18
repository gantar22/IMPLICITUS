using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TypeUtil
{
    [System.Serializable]
    public struct Shrub<T>
    {
        
        private enum Injection : byte {node,leaf}

        private readonly Injection injection;
        private readonly object val;


        private Shrub(Injection injection, object val)
        {
            this.injection = injection;
            this.val = val;
        }

        public T2 Match<T2>(Func<List<Shrub<T>>, T2> l, Func<T, T2> r)
        {
            switch (injection)
            {
                case Injection.node:
                    return l(((List<Shrub<T>>)val).ToList());
                case Injection.leaf:
                    return r((T) val);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Match(Action<List<Shrub<T>>> l, Action<T> r)
        {
            Match(a =>
            {
                l(a);
                return new Unit();
            }, b =>
            {
                r(b);
                return new Unit();
            });
        }
        
        public static Shrub<T> Leaf(T t)
        {
            return new Shrub<T>(Injection.leaf,t);
        }

        public static Shrub<T> Node(List<Shrub<T>> L)
        {
            return new Shrub<T>(Injection.node,L);
        }


        public Shrub<T2> Map<T2>(Func<T,T2> f)
        {
            return Match(
                l => Shrub<T2>.Node(l.Select(s => s.Map<T2>(f)).ToList())
                ,
                v => Shrub<T2>.Leaf(f(v)));
        }

        public Shrub<T2> MapI<T2>(List<int> p,Func<T,List<int>,T2> f)
        {
            return Match(
                l => Shrub<T2>.Node(l.Select((s,i) => s.MapI<T2>(p.Append(i).ToList(),f)).ToList())
                ,
                v => Shrub<T2>.Leaf(f(v,p)));
        }

        public void IterateI(List<int> p, Action<T, List<int>> f)
        {
            MapI(p, (v, l) =>
            {
                f(v, l);
                return new Unit();
            });
        }
        
        
        public Shrub<T> ApplyAt(Func<Shrub<T>, Shrub<T>> f, List<int> path)
        {
            if (path.Count == 0)
            {
                return f(this);
            }
            else
            {
                int i = path[0];
                return Match(
                l => Node(l.Take(i). Append(l[i].ApplyAt(f,path.Skip(1).ToList())) .Concat(l.Skip(i + 1)).ToList()),
                Leaf
                );
            }
        }

        public Shrub<T> Access(List<int> p)
        {
            if (p.Count == 0)
                return this;
            return Match(l => l[p[0]].Access(p.Skip(0).ToList()), v => throw new IndexOutOfRangeException());
        }
        
        public List<T> Preorder()
        {
            return Match(
                l => l.SelectMany(s => s.Preorder()).ToList(),
                v => new List<T>().Append(v).ToList()
                );
        }

        public List<T2> MapPreorder<T2>(Func<T,T2> f,Func<List<T2>, List<T2>> fs)
        {
            return Match(
                l => fs(l.SelectMany(s => s.MapPreorder<T2>(f,fs)).ToList()),
                v => new List<T2>().Append(f(v)).ToList()
                );
        }


        public override string ToString()
        {
            return String.Join(" ",MapPreorder(t => t.ToString(), l => l.Prepend("(").Append(")").ToList()));
        }

        public bool IsNode()
        {
            return Match(l => true, u => false);
        }
        

        public static Shrub<T2> Collapse<T2>(Shrub<Shrub<T2>> input)
        {
            return input.Match<Shrub<T2>>(
                l => Shrub<T2>.Node(l.Select(Collapse<T2>).ToList()),
                v => v.Match(Shrub<T2>.Node,Shrub<T2>.Leaf)
                );
        }
    }   

}