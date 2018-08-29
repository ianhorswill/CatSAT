#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Fluents.cs" company="Ian Horswill">
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
using System.Linq;
using static CatSAT.Language;

namespace CatSAT
{
    public static class Fluents
    {
        /// <summary>
        /// Time horizon of the current problem.
        /// </summary>
        public static int TimeHorizon => Problem.Current.TimeHorizon;
        /// <summary>
        /// Timepoints in the time horizon of the current problem
        /// </summary>
        public static IEnumerable<int> TimePoints => Enumerable.Range(0, TimeHorizon);
        /// <summary>
        /// All timepoints in the time horizon of the current problem, except the last one
        /// </summary>
        public static IEnumerable<int> ActionTimePoints => Enumerable.Range(0, TimeHorizon-1);

        //delegate FluentInstantiation FluentT(int time);
        //delegate Proposition FluentT<T1>(T1 arg1, int time);
        //delegate Proposition FluentT<T1, T2>(T1 arg1, T2 arg2, int time);
        //delegate Proposition FluentT<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, int time);

        public class FluentInstantiation : SpecialProposition
        {
        }

        public static readonly Func<FluentInstantiation, Proposition> Activate = Predicate<FluentInstantiation>("activate");
        public static readonly Func<FluentInstantiation, Proposition> Deactivate = Predicate<FluentInstantiation>("deactivate");

        public static Func<int, FluentInstantiation> Fluent(string name, 
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;

            ValidateTimeHorizaon(problem);

            var f = PredicateOfType<int, FluentInstantiation>(name);
            for (int i = 1; i < problem.TimeHorizon; i++)
            {
                var before = f(i - 1);
                var after = f(i);
                AddFluentClauses(problem, requireActivationSupport, requireDeactivationSupport, before, after);
            }
            return f;
        }

        public static Func<T, int, FluentInstantiation> Fluent<T>(string name, ICollection<T> domain,
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;
            ValidateTimeHorizaon(problem);
            var f = PredicateOfType<T, int, FluentInstantiation>(name);
            foreach (var d in domain)
            {
                for (int i = 1; i < problem.TimeHorizon; i++)
                {
                    var before = f(d, i - 1);
                    var after = f(d, i);
                    AddFluentClauses(problem, requireActivationSupport, requireDeactivationSupport, before, after);
                }
            }

            return f;
        }

        public static Func<T1, T2, int, FluentInstantiation> Fluent<T1, T2>(string name, ICollection<T1> domain1, ICollection<T2> domain2,
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;

            ValidateTimeHorizaon(problem);

            var f = PredicateOfType<T1, T2, int, FluentInstantiation>(name);
            foreach (var d1 in domain1)
                foreach (var d2 in domain2)
            {
                for (int i = 1; i < problem.TimeHorizon; i++)
                {
                    var before = f(d1, d2, i - 1);
                    var after = f(d1, d2, i);
                    AddFluentClauses(problem, requireActivationSupport, requireDeactivationSupport, before, after);
                }
            }

            return f;
        }

        public static Func<T, T, int, FluentInstantiation> SymmetricFluent<T>(string name, ICollection<T> domain1,
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        where T : IComparable
        {
            if (problem == null) problem = Problem.Current;

            ValidateTimeHorizaon(problem);

            var f = SymmetricPredicateOfType<T, int, FluentInstantiation>(name);
            foreach (var d1 in domain1)
            foreach (var d2 in domain1)
            {
                for (int i = 1; i < problem.TimeHorizon; i++)
                {
                    var before = f(d1, d2, i - 1);
                    var after = f(d1, d2, i);
                    AddFluentClauses(problem, requireActivationSupport, requireDeactivationSupport, before, after);
                }
            }

            return f;
        }

        private static void AddFluentClauses(Problem problem, bool requireActivationSupport, bool requireDeactivationSupport,
            FluentInstantiation before, FluentInstantiation after)
        {
            var activate = Activate(before);
            if (requireActivationSupport)
                activate.RequireHaveSupport();
            var deactivate = Deactivate(before);
            if (requireDeactivationSupport)
                deactivate.RequireHaveSupport();

            // TODO - optimize these to generate signed indices directly; this will reduce garbage

            // activate => after
            problem.AddClause(after, Not(activate));
            // deactivate => not after
            problem.AddClause(Not(after), Not(deactivate));
            // before => after | deactivate
            problem.AddClause(Not(before), after, deactivate);
            // not before => not after | activate
            problem.AddClause(before, Not(after), activate);
            // Can't simultaneously activate and deactivate
            problem.AddClause(0, 1, activate, deactivate);
        }

        private static void ValidateTimeHorizaon(Problem problem)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }

            if (problem.TimeHorizon < 0)
                throw new InvalidOperationException("Attempt to create Fluent for a Problem with no TimeHorizon");
        }
    }
}
