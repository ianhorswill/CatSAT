#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Variable.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using CatSAT;

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
            get => Domain.Elements;
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
