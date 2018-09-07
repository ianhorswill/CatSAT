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
    /// <summary>
    /// Represents an action that can modify a fluent
    /// Actions are functions that map time points, and optionally other arguments to propositions representing that
    /// that action has occurred at that time.
    /// </summary>
    public static class Actions
    {
        /// <summary>
        /// Represents an application of an action to a specific set of arguments at a specific point in time
        /// </summary>
        public class ActionInstantiation : SpecialProposition { }
        /// <summary>
        /// Represents an application of an action to a specifc set of arguments at a specifc point in time
        /// </summary>
        public class SymmetricActionInstantiation : ActionInstantiation { }

        static readonly Dictionary<object, object> Domain = new Dictionary<object, object>();

        /// <summary>
        /// Creates a new atomic action within a CatSAT Problem
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <returns>The action</returns>
        public static Func<int, ActionInstantiation> Action(string name)
        {
            return PredicateOfType<int, ActionInstantiation>(name);
        }

        /// <summary>
        /// Enumeration of all possible instances of action at the specified timepoint
        /// </summary>
        /// <param name="a">Action</param>
        /// <param name="t">Timepoint</param>
        /// <returns>Instances</returns>
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<ActionInstantiation> Instances(Func<int, ActionInstantiation> a, int t)
        {
            yield return a(t);
        }

        /// <summary>
        /// Creates a new atomic action within a CatSAT Problem
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <param name="domain1">Domain for first argument</param>
        /// <returns>The action</returns>
        public static Func<T1, int, ActionInstantiation> Action<T1>(string name, ICollection<T1> domain1)
        {
            var a = PredicateOfType<T1, int, ActionInstantiation>(name);
            Domain[a] = domain1;
            return a;
        }

        /// <summary>
        /// Maps function over the domain of the action
        /// </summary>
        /// <param name="a">Action over whose domain to map</param>
        /// <param name="f">Function to apply to the domain</param>
        /// <typeparam name="T1">Type of the action's argument</typeparam>
        /// <typeparam name="TOut">Result type of the function</typeparam>
        /// <returns>Stream of results of the function on the domain elements</returns>
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<TOut> MapDomain<T1, TOut>(Func<T1, int, ActionInstantiation> a, Func<T1, TOut> f)
        {
            foreach (var arg in (IEnumerable<T1>) Domain[a])
                yield return f(arg);
        }

        /// <summary>
        /// Enumeration of all possible instances of action at the specified timepoint
        /// </summary>
        /// <param name="a">Action</param>
        /// <param name="t">Timepoint</param>
        /// <returns>Instances</returns>
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<ActionInstantiation> Instances<T1>(Func<T1, int, ActionInstantiation> a, int t)
        {
            foreach (var arg in (IEnumerable<T1>)Domain[a])
                yield return a(arg, t);
        }

        /// <summary>
        /// Creates a new atomic action within a CatSAT Problem
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <param name="domain1">Domain for first argument</param>
        /// <param name="domain2">Domain for second argument</param>
        /// <returns>The action</returns>
        public static Func<T1, T2, int, ActionInstantiation> Action<T1, T2>(string name, ICollection<T1> domain1, ICollection<T2> domain2)
        {
            var a = PredicateOfType<T1, T2, int, ActionInstantiation>(name);
            Domain[a] = new Tuple<ICollection<T1>, ICollection<T2>>(domain1, domain2);
            return a;
        }

        /// <summary>
        /// Maps function over the domain of the action
        /// </summary>
        /// <param name="a">Action over whose domain to map</param>
        /// <param name="f">Function to apply to the domain</param>
        /// <typeparam name="T1">Type of the action's argument</typeparam>
        /// <typeparam name="T2">Type of the action's second argument</typeparam>
        /// <typeparam name="TOut">Result type of the function</typeparam>
        /// <returns>Stream of results of the function on the domain elements</returns>
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

        /// <summary>
        /// Enumeration of all possible instances of action at the specified timepoint
        /// </summary>
        /// <param name="a">Action</param>
        /// <param name="t">Timepoint</param>
        /// <returns>Instances</returns>
        public static IEnumerable<ActionInstantiation> Instances<T1, T2>(Func<T1, T2, int, ActionInstantiation> a,
            int t)
        {
            return MapDomain(a, (arg1, arg2) => a(arg1, arg2, t));
        }

        /// <summary>
        /// Creates a new atomic action within a CatSAT Problem
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <param name="domain">Domain for first and second arguments</param>
        /// <returns>The action</returns>
        public static Func<T, T, int, SymmetricActionInstantiation> SymmetricAction<T>(string name, ICollection<T> domain)
            where T : IComparable
        {
            var a = SymmetricPredicateOfType<T, int, SymmetricActionInstantiation>(name);
            Domain[a] = domain;
            return a;
        }

        /// <summary>
        /// Maps function over the domain of the action
        /// </summary>
        /// <param name="a">Action over whose domain to map</param>
        /// <param name="f">Function to apply to the domain</param>
        /// <typeparam name="T1">Type of the action's arguments</typeparam>
        /// <typeparam name="TOut">Result type of the function</typeparam>
        /// <returns>Stream of results of the function on the domain elements</returns>
        public static IEnumerable<TOut> MapDomain<T1, TOut>(Func<T1, T1, int, SymmetricActionInstantiation> a, Func<T1, T1, TOut> f)
        where T1:IComparable
        {
            var domain = (ICollection<T1>)Domain[a];
            foreach (var arg1 in domain)
            foreach (var arg2 in domain)
               if (arg1.CompareTo(arg2) <= 0)
                    yield return f(arg1, arg2);
        }

        /// <summary>
        /// Enumeration of all possible instances of action at the specified timepoint
        /// </summary>
        /// <param name="a">Action</param>
        /// <param name="t">Timepoint</param>
        /// <returns>Instances</returns>
        public static IEnumerable<SymmetricActionInstantiation> Instances<T1>(Func<T1, T1, int, SymmetricActionInstantiation> a,
            int t)
        where T1 : IComparable
        {
            return MapDomain(a, (arg1, arg2) => a(arg1, arg2, t));
        }

        /// <summary>
        /// Asserts that the specified fluent is a precondition of the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="precondition">Fluent that must be true at a given timepoint for the action to be runnable.</param>
        // ReSharper disable once UnusedMember.Global
        public static void Precondition(Func<int, ActionInstantiation> action, Func<int, Proposition> precondition)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(action(t) > precondition(t));
        }

        /// <summary>
        /// Asserts that the specified fluent is a precondition of the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="precondition">Fluent that must be true at a given timepoint for the action to be runnable.</param>
        // ReSharper disable once UnusedMember.Global
        public static void Precondition<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, Proposition> precondition)
        {
            var domain1 = (ICollection<T1>) Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(action(d1, t) > precondition(d1, t));
        }

        /// <summary>
        /// Asserts that the specified fluent is a precondition of the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="precondition">Fluent that must be true at a given timepoint for the action to be runnable.</param>
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

        /// <summary>
        /// Asserts that the specified fluent is a precondition of the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="precondition">Fluent that must be true at a given timepoint for the action to be runnable.</param>
        public static void Precondition<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, Literal> precondition)
        where T1:IComparable
        {
            foreach (var t in ActionTimePoints)
                // ReSharper disable once IteratorMethodResultIsIgnored
                MapDomain(action,
                (d1, d2) =>
                {
                    Problem.Current.Assert(action(d1, d2, t) > precondition(d1, d2, t));
                    return false;
                });
        }

        /// <summary>
        /// Asserts the specified action activates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being activated</param>
        // ReSharper disable once UnusedMember.Global
        public static void Adds(Func<int, ActionInstantiation> action, Func<int, FluentInstantiation> effect)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Activate(effect(t)) <= action(t));
        }

        /// <summary>
        /// Asserts the specified action activates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being activated</param>
        // ReSharper disable once UnusedMember.Global
        public static void Adds<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, FluentInstantiation> effect)
        {
            var domain1 = (ICollection<T1>)Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Activate(effect(d1, t)) <= action(d1, t));
        }

        /// <summary>
        /// Asserts the specified action activates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being activated</param>
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

        /// <summary>
        /// Asserts the specified action activates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being activated</param>
        public static void Adds<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, FluentInstantiation> effect)
            where T1 : IComparable
        {
            foreach (var t in ActionTimePoints)
                // ReSharper disable once IteratorMethodResultIsIgnored
                MapDomain(action,
                    (d1, d2) =>
                    {
                        Problem.Current.Assert(Activate(effect(d1, d2, t)) <= action(d1, d2, t));
                        return false;
                    });
        }

        /// <summary>
        /// Asserts the specified action deactivates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being deactivated</param>
        // ReSharper disable once UnusedMember.Global
        public static void Deletes(Func<int, ActionInstantiation> action, Func<int, FluentInstantiation> effect)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Deactivate(effect(t)) <= action(t));
        }

        /// <summary>
        /// Asserts the specified action deactivates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being deactivated</param>
        // ReSharper disable once UnusedMember.Global
        public static void Deletes<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, FluentInstantiation> effect)
        {
            var domain1 = (ICollection<T1>)Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert(Deactivate(effect(d1, t)) <= action(d1, t));
        }

        /// <summary>
        /// Asserts the specified action deactivates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being deactivated</param>
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

        /// <summary>
        /// Asserts the specified action deactivates the specified fluent
        /// </summary>
        /// <param name="action">Action that changes the fluent</param>
        /// <param name="effect">Fluent being deactivated</param>
        // ReSharper disable once UnusedMember.Global
        public static void Deletes<T1>(Func<T1, T1, int, SymmetricActionInstantiation> action,
            Func<T1, T1, int, FluentInstantiation> effect)
            where T1 : IComparable
        {
            foreach (var t in ActionTimePoints)
                // ReSharper disable once IteratorMethodResultIsIgnored
                MapDomain(action,
                    (d1, d2) =>
                    {
                        Problem.Current.Assert(Deactivate(effect(d1, d2, t)) <= action(d1, d2, t));
                        return false;
                    });
        }
    }
}
