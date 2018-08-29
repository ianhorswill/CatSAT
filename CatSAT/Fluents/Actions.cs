#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Actions.cs" company="Ian Horswill">
// Copyright (C) 2018 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using static CatSAT.Language;
using static CatSAT.Fluents;

namespace CatSAT
{
    public static class Actions
    {
        public class ActionInstantiation : SpecialProposition { }
        public class SymmetricActionInstantiation : ActionInstantiation { }

        static readonly Dictionary<object, object> Domain = new Dictionary<object, object>();

        public static Func<int, ActionInstantiation> Action(string name)
        {
            return PredicateOfType<int, ActionInstantiation>(name);
        }

        public static IEnumerable<ActionInstantiation> Instances(Func<int, ActionInstantiation> a, int t)
        {
            yield return a(t);
        }

        public static Func<T1, int, ActionInstantiation> Action<T1>(string name, ICollection<T1> domain1)
        {
            var a = PredicateOfType<T1, int, ActionInstantiation>(name);
            Domain[a] = domain1;
            return a;
        }

        public static IEnumerable<TOut> MapDomain<T1, TOut>(Func<T1, int, ActionInstantiation> a, Func<T1, TOut> f)
        {
            foreach (var arg in (IEnumerable<T1>) Domain[a])
                yield return f(arg);
        }

        public static IEnumerable<ActionInstantiation> Instances<T1>(Func<T1, int, ActionInstantiation> a, int t)
        {
            foreach (var arg in (IEnumerable<T1>)Domain[a])
                yield return a(arg, t);
        }

        public static Func<T1, T2, int, ActionInstantiation> Action<T1, T2>(string name, ICollection<T1> domain1, ICollection<T2> domain2)
        {
            var a = PredicateOfType<T1, T2, int, ActionInstantiation>(name);
            Domain[a] = new Tuple<ICollection<T1>, ICollection<T2>>(domain1, domain2);
            return a;
        }

        public static IEnumerable<TOut> MapDomain<T1, T2, TOut>(Func<T1, T2, int, ActionInstantiation> a,
            Func<T1, T2, TOut> f)
        {
            var domain = (Tuple<ICollection<T1>, ICollection<T2>>) Domain[a];
            var domain1 = domain.Item1;
            var domain2 = domain.Item2;
            foreach (var arg1 in domain1)
            foreach (var arg2 in domain2)
                yield return f(arg1, arg2);
        }

        public static IEnumerable<ActionInstantiation> Instances<T1, T2>(Func<T1, T2, int, ActionInstantiation> a,
            int t)
        {
            return MapDomain(a, (arg1, arg2) => a(arg1, arg2, t));
        }

        public static Func<T, T, int, SymmetricActionInstantiation> SymmetricAction<T>(string name, ICollection<T> domain)
            where T : IComparable
        {
            var a = SymmetricPredicateOfType<T, int, SymmetricActionInstantiation>(name);
            Domain[a] = domain;
            return a;
        }

        public static IEnumerable<TOut> MapDomain<T1, TOut>(Func<T1, T1, int, SymmetricActionInstantiation> a, Func<T1, T1, TOut> f)
        where T1:IComparable
        {
            var domain = (ICollection<T1>)Domain[a];
            foreach (var arg1 in domain)
            foreach (var arg2 in domain)
               if (arg1.CompareTo(arg2) <= 0)
                    yield return f(arg1, arg2);
        }

        public static IEnumerable<SymmetricActionInstantiation> Instances<T1>(Func<T1, T1, int, SymmetricActionInstantiation> a,
            int t)
        where T1 : IComparable
        {
            return MapDomain(a, (arg1, arg2) => a(arg1, arg2, t));
        }

        public static void Precondition(Func<int, ActionInstantiation> action, Func<int, Proposition> precondition)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(action(t) > precondition(t));
        }

        public static void Precondition<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, Proposition> precondition)
        {
            var domain1 = (ICollection<T1>) Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(action(d1, t) > precondition(d1, t));
        }

        public static void Precondition<T1, T2>(Func<T1, T2, int, ActionInstantiation> action,
            Func<T1, T2, int, Literal> precondition)
        {
            var domain = (Tuple<ICollection<T1>, ICollection<T2>>) Domain[action];
            var domain1 = domain.Item1;
            var domain2 = domain.Item2;

            foreach (var d1 in domain1)
            foreach (var d2 in domain2)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(action(d1, d2, t) > precondition(d1, d2, t));
        }

        public static void Precondition<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, Literal> precondition)
        where T1:IComparable
        {
            foreach (var t in ActionTimePoints)
            MapDomain<T1,bool>(action,
                (d1, d2) =>
                {
                    Problem.Current.Assert(action(d1, d2, t) > precondition(d1, d2, t));
                    return false;
                });
        }

        public static void Adds(Func<int, ActionInstantiation> action, Func<int, FluentInstantiation> effect)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Activate(effect(t)) <= action(t));
        }

        public static void Adds<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, FluentInstantiation> effect)
        {
            var domain1 = (ICollection<T1>)Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Activate(effect(d1, t)) <= action(d1, t));
        }

        public static void Adds<T1, T2>(Func<T1, T2, int, ActionInstantiation> action,
            Func<T1, T2, int, FluentInstantiation> effect)
        {
            var domain = (Tuple<ICollection<T1>, ICollection<T2>>)Domain[action];
            var domain1 = domain.Item1;
            var domain2 = domain.Item2;

            foreach (var d1 in domain1)
            foreach (var d2 in domain2)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Activate(effect(d1, d2, t)) <= action(d1, d2, t));
        }

        public static void Adds<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, FluentInstantiation> effect)
            where T1 : IComparable
        {
            foreach (var t in ActionTimePoints)
                MapDomain<T1, bool>(action,
                    (d1, d2) =>
                    {
                        Problem.Current.Assert(Activate(effect(d1, d2, t)) <= action(d1, d2, t));
                        return false;
                    });
        }

        public static void Deletes(Func<int, ActionInstantiation> action, Func<int, FluentInstantiation> effect)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Deactivate(effect(t)) <= action(t));
        }

        public static void Deletes<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, FluentInstantiation> effect)
        {
            var domain1 = (ICollection<T1>)Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Deactivate(effect(d1, t)) <= action(d1, t));
        }

        public static void Deletes<T1, T2>(Func<T1, T2, int, ActionInstantiation> action,
            Func<T1, T2, int, FluentInstantiation> effect)
        {
            var domain = (Tuple<ICollection<T1>, ICollection<T2>>)Domain[action];
            var domain1 = domain.Item1;
            var domain2 = domain.Item2;

            foreach (var d1 in domain1)
            foreach (var d2 in domain2)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Deactivate(effect(d1, d2, t)) <= action(d1, d2, t));
        }

        public static void Deletes<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, FluentInstantiation> effect)
            where T1 : IComparable
        {
            foreach (var t in ActionTimePoints)
                MapDomain<T1, bool>(action,
                    (d1, d2) =>
                    {
                        Problem.Current.Assert(Deactivate(effect(d1, d2, t)) <= action(d1, d2, t));
                        return false;
                    });
        }
    }
}
