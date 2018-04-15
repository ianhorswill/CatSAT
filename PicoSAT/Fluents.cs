using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                var activate = Activate(before);
                if (requireActivationSupport)
                    activate.RequireHaveSupport();
                var deactivate = Deactivate(before);
                if (requireDeactivationSupport)
                    deactivate.RequireHaveSupport();
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
            return f;
        }

        public static Func<T, int, Proposition> Fluent<T>(string name, IEnumerable<T> domain,
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
                    var activate = Activate(before);
                    if (requireActivationSupport)
                        activate.RequireHaveSupport();
                    var deactivate = Deactivate(before);
                    if (requireDeactivationSupport)
                        deactivate.RequireHaveSupport();
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
            }

            return f;
        }

        public static Func<T1, T2, int, Proposition> Fluent<T1, T2>(string name, IEnumerable<T1> domain1, IEnumerable<T2> domain2,
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
                    var activate = Activate(before);
                    if (requireActivationSupport)
                        activate.RequireHaveSupport();
                    var deactivate = Deactivate(before);
                    if (requireDeactivationSupport)
                        deactivate.RequireHaveSupport();
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
            }

            return f;
        }

        private static void ValidateTimeHorizaon(Problem problem)
        {
            if (problem.TimeHorizon < 0)
                throw new InvalidOperationException("Attempt to create Fluent for a Problem with no TimeHorizon");
        }
    }
}
