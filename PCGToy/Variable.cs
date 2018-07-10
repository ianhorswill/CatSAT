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
        public FDomain<object> Domain => Problem.Domains[DomainName];

        public IList<object> DomainValues
        {
            get => Domain.Values;
            set
            {
                Problem.Domains[DomainName] = new FDomain<object>(DomainName, value);
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
                SolverVariable.SetPredeterminedValue(value);
            }
        }

        public void Unbind()
        {
            SolverVariable.Reset();
        }

        public bool IsLocked;

        public FDVariable<object> SolverVariable;

        public Variable(string name, PCGProblem p, string domainName, Condition condition)
        {
            Name = name;
            Problem = p;
            DomainName = domainName;
            Condition = condition;
            Parent?.Children.Add(this);

            if (DomainValues.Count> 0)
                _value = DomainValues[0];
            IsLocked = false;
        }

        public void CompileToProblem(Problem p)
        {
            if (DomainValues.Count > 0)
            {
                SolverVariable = Condition == null? new FDVariable<object>(Name, Domain):new FDVariable<object>(Name, Domain, Condition.Literal);
                if (IsLocked)
                    SolverVariable.SetPredeterminedValue(Value);
            }
        }

        public void UpdateFromSolution(Solution s)
        {
            if (SolverVariable.IsDefinedIn(s))
                _value = SolverVariable.Value(s);
            else
                _value = null;
        }
    }
}
