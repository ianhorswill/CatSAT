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
        public static int TimeHorizon;

        public static readonly Func<Proposition, Proposition> Activate = Predicate<Proposition>("activate");
        public static readonly Func<Proposition, Proposition> Deactivate = Predicate<Proposition>("deactivate");

        public static Func<int, Proposition> Fluent(string name)
        {
            var p = Problem.Current;
            var f = Predicate<int>(name);
            for (int i = 1; i < TimeHorizon; i++)
            {
                var before = f(i - 1);
                var after = f(i);
                var activate = Activate(before);
                var deactivate = Deactivate(before);
                // activate => after
                p.AddClause(after, Not(activate));
                // deactivate => not after
                p.AddClause(Not(after), Not(deactivate));
                // before => after | deactivate
                p.AddClause(Not(before), after, deactivate);
                // not before => not after | activate
                p.AddClause(before, Not(after), activate);
                // Can't simultaneously activate and deactivate
                p.AddClause(0, 1, activate, deactivate);
            }
            return f;
        }

        public static Func<T, int, Proposition> Fluent<T>(string name, IEnumerable<T> domain)
        {
            var p = Problem.Current;
            var f = Predicate<T, int>(name);
            foreach (var d in domain)
            {
                for (int i = 1; i < TimeHorizon; i++)
                {
                    var before = f(d, i - 1);
                    var after = f(d, i);
                    var activate = Activate(before);
                    var deactivate = Deactivate(before);
                    // activate => after
                    p.AddClause(after, Not(activate));
                    // deactivate => not after
                    p.AddClause(Not(after), Not(deactivate));
                    // before => after | deactivate
                    p.AddClause(Not(before), after, deactivate);
                    // not before => not after | activate
                    p.AddClause(before, Not(after), activate);
                    // Can't simultaneously activate and deactivate
                    p.AddClause(0, 1, activate, deactivate);
                }
            }

            return f;
        }

        public static Func<T1, T2, int, Proposition> Fluent<T1, T2>(string name, IEnumerable<T1> domain1, IEnumerable<T2> domain2)
        {
            var p = Problem.Current;
            var f = Predicate<T1, T2, int>(name);
            foreach (var d1 in domain1)
                foreach (var d2 in domain2)
            {
                for (int i = 1; i < TimeHorizon; i++)
                {
                    var before = f(d1, d2, i - 1);
                    var after = f(d1, d2, i);
                    var activate = Activate(before);
                    var deactivate = Deactivate(before);
                    // activate => after
                    p.AddClause(after, Not(activate));
                    // deactivate => not after
                    p.AddClause(Not(after), Not(deactivate));
                    // before => after | deactivate
                    p.AddClause(Not(before), after, deactivate);
                    // not before => not after | activate
                    p.AddClause(before, Not(after), activate);
                    // Can't simultaneously activate and deactivate
                    p.AddClause(0, 1, activate, deactivate);
                }
            }

            return f;
        }
    }
}
