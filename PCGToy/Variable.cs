using System;
using System.Collections.Generic;
using System.Linq;
using PicoSAT;
using static PicoSAT.Language;

namespace PCGToy
{
    public class Variable
    {
        public readonly string Name;
        public readonly PCGProblem Problem;
        public readonly string DomainName;

        public object[] Domain
        {
            get => Problem.Domains[DomainName];
            set
            {
                Problem.Domains[DomainName] = value;
                Problem.Changed();
            }
        }
        public readonly Condition Condition;

        /// <summary>
        /// The variable this variable is dependent on, if any
        /// </summary>
        public Variable Parent => Condition?.Variable;

        /// <summary>
        /// The variables of which this is a parent.
        /// </summary>
        public List<Variable> Children = new List<Variable>();

        private object _value;

        public string NameAndValue => $"{Name} = {Value??"null"}";

        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                Unbind();
                Problem.Problem[predicate(value)] = true;
            }
        }

        public void Unbind()
        {
            foreach (var d in Domain)
                Problem.Problem.ResetProposition(predicate(d));
        }

        public bool IsLocked;

        internal Func<object, Proposition> predicate;

        public Variable(string name, PCGProblem p, string domainName, Condition condition)
        {
            Name = name;
            Problem = p;
            DomainName = domainName;
            Condition = condition;
            Parent?.Children.Add(this);

            if (Domain.Length > 0)
                _value = Domain[0];
            IsLocked = false;
        }

        public void CompileToProblem(Problem p)
        {
            predicate = Predicate<object>(Name);
            if (Domain.Length > 0)
            {
                var generator = Domain.Select(predicate).Cast<Literal>();
                var items = Condition == null ? generator : generator.Concat(new[] {Not(Condition.Literal)});
                p.Unique(items);
                if (IsLocked)
                    p[predicate(Value)] = true;
            }
        }

        public void UpdateFromSolution(Solution s)
        {
            foreach (var v in Domain)
                if (s[predicate(v)])
                {
                    _value = v;
                    return;
                }

            _value = null;
        }
    }
}
