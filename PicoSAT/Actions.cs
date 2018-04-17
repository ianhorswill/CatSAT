using System;
using System.Collections.Generic;
using static PicoSAT.Language;
using static PicoSAT.Fluents;

namespace PicoSAT
{
    public static class Actions
    {
        public class ActionInstantiation : Proposition { }

        static Dictionary<object, object> Domain = new Dictionary<object, object>();

        public static Func<int, ActionInstantiation> Action(string name)
        {
            return PredicateOfType<int, ActionInstantiation>(name);
        }

        public static Func<T1, int, ActionInstantiation> Action<T1>(string name, ICollection<T1> domain1)
        {
            var a = PredicateOfType<T1, int, ActionInstantiation>(name);
            Domain[a] = domain1;
            return a;
        }

        public static Func<T1, T2, int, ActionInstantiation> Action<T1, T2>(string name, ICollection<T1> domain1, ICollection<T2> domain2)
        {
            var a = PredicateOfType<T1, T2, int, ActionInstantiation>(name);
            Domain[a] = new Tuple<ICollection<T1>, ICollection<T2>>(domain1, domain2);
            return a;
        }

        public static void Precondition(Func<int, ActionInstantiation> action, Func<int, Proposition> precondition)
        {
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert((Expression)action(t) >= precondition(t));
        }

        public static void Precondition<T1>(Func<T1, int, ActionInstantiation> action, Func<T1, int, Proposition> precondition)
        {
            var domain1 = (ICollection<T1>) Domain[action];
            foreach (var d1 in domain1)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert((Expression)action(d1, t) >= precondition(d1, t));
        }

        public static void Precondition<T1, T2>(Func<T1, T2, int, ActionInstantiation> action,
            Func<T1, T2, int, Proposition> precondition)
        {
            var domain = (Tuple<ICollection<T1>, ICollection<T2>>) Domain[action];
            var domain1 = domain.Item1;
            var domain2 = domain.Item2;

            foreach (var d1 in domain1)
            foreach (var d2 in domain2)
            foreach (var t in ActionTimePoints)
                Problem.Current.Assert((Expression) action(d1, d2, t) >= precondition(d1, d2, t));
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
    }
}
