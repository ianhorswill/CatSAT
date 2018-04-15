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
using static PicoSAT.Language;

namespace PicoSAT
{
    public static class Fluents
    {
        public static readonly Func<Proposition, Proposition> Activate = Predicate<Proposition>("activate");
        public static readonly Func<Proposition, Proposition> Deactivate = Predicate<Proposition>("deactivate");

        public static Func<int, Proposition> Fluent(string name, 
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;

            ValidateTimeHorizaon(problem);

            var f = Predicate<int>(name);
            for (int i = 1; i < problem.TimeHorizon; i++)
            {
                var before = f(i - 1);
                var after = f(i);
                AddFluentClauses(problem, requireActivationSupport, requireDeactivationSupport, before, after);
            }
            return f;
        }

        public static Func<T, int, Proposition> Fluent<T>(string name, ICollection<T> domain,
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;
            ValidateTimeHorizaon(problem);
            var f = Predicate<T, int>(name);
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

        public static Func<T1, T2, int, Proposition> Fluent<T1, T2>(string name, ICollection<T1> domain1, ICollection<T2> domain2,
            Problem problem = null, bool requireActivationSupport = true, bool requireDeactivationSupport = true)
        {
            if (problem == null) problem = Problem.Current;

            ValidateTimeHorizaon(problem);

            var f = Predicate<T1, T2, int>(name);
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

        private static void AddFluentClauses(Problem problem, bool requireActivationSupport, bool requireDeactivationSupport,
            Proposition before, Proposition after)
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
