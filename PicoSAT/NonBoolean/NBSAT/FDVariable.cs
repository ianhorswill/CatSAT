#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FDVariable.cs" company="Ian Horswill">
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
using System.Diagnostics;
using System.Linq;

namespace PicoSAT
{
#pragma warning disable 660,661
    // ReSharper disable once InconsistentNaming
    public class FDVariable<T> : DomainVariable<T>
#pragma warning restore 660,661
    {
        /// <summary>
        /// Domain of the variable
        /// </summary>
        protected readonly FDomain<T> domain;
        /// <inheritdoc />
        public override Domain<T> Domain => domain;
        /// <summary>
        /// Individual propositions asserting the different possible values of the variable.
        /// </summary>
        protected readonly Proposition[] valuePropositions;

        /// <summary>
        /// Make a new finite-domain variable to solve for
        /// </summary>
        /// <param name="name">"Name" to give to the variable (need not be a string)</param>
        /// <param name="domain">Domain of the variable</param>
        /// <param name="condition">Condition under which the variable is defined (null = true)</param>
        public FDVariable(object name, FDomain<T> domain, Literal condition = null) :
            this(name, domain, condition, Problem.Current)
        { }

        /// <summary>
        /// Make a new finite-domain variable to solve for
        /// </summary>
        /// <param name="name">"Name" to give to the variable (need not be a string)</param>
        /// <param name="domain">Domain of the variable</param>
        /// <param name="condition">Condition under which the variable is defined (null = true)</param>
        /// <param name="problem">Problem in which to define the variable</param>
        public FDVariable(object name, FDomain<T> domain, Literal condition, Problem problem)
            : base(name, problem, condition)
        {
            this.domain = domain;
            valuePropositions = domain.Values.Select(v => problem.GetInternalProposition(new ValueProposition(this, v))).ToArray();
            if ((object)condition == null)
                problem.Unique((IEnumerable<Literal>)valuePropositions);
            else
                problem.Unique(valuePropositions.Concat(new[] { Language.Not(condition)}));
        }

        /// <summary>
        /// Make a new finite-domain variable to solve for
        /// </summary>
        /// <param name="name">"Name" to give to the variable (need not be a string)</param>
        /// <param name="domainValues">Allowed values of the variable</param>
        public FDVariable(object name, params T[] domainValues)
            : this(name, null, domainValues)
        { }

        /// <summary>
        /// Make a new finite-domain variable to solve for
        /// </summary>
        /// <param name="name">"Name" to give to the variable (need not be a string)</param>
        /// <param name="condition">Condition under which the variable is defined (null = true)</param>
        /// <param name="domainValues">Allowed values of the variable</param>
        public FDVariable(object name, Literal condition, params T[] domainValues) :
            this(name, new FDomain<T>(name.ToString(), domainValues), condition)
        { }


        /// <summary>
        /// A Proposition asserting that the variable has a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator ==(FDVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            return var.valuePropositions[var.domain.IndexOf(value)];
        }

        /// <summary>
        /// A Proposition asserting that the variable does not have a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator !=(FDVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            return Language.Not(var.valuePropositions[var.domain.IndexOf(value)]);
        }

        public override Literal EqualityProposition(object value)
        {
            if (value is T tValue)
                return this == tValue;
            throw new ArgumentException($"{value} is not a valid value for FDVariable {Name}");
        }

        public override bool IsDefinedIn(Solution s)
        {
            foreach (var p in valuePropositions)
                if (s[p])
                    return true;
            return false;
        }

        /// <inheritdoc />
        public override T Value(Solution s)
        {
            foreach (var p in valuePropositions)
                if (s[p])
                    return ((ValueProposition) p.Name).Value;
            throw new InvalidOperationException($"{Name} has no value assigned.");
        }

        /// <inheritdoc />
        public override T PredeterminedValue()
        {
            foreach (var p in valuePropositions)
                if (Problem[p])
                    return ((ValueProposition)p.Name).Value;
            throw new InvalidOperationException($"{Name} has no value assigned.");
        }

        /// <inheritdoc />
        public override void SetPredeterminedValue(T newValue)
        {
            foreach (var p in valuePropositions)
                Problem[p] = ((ValueProposition)p.Name).Value.Equals(newValue);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            foreach (var p in valuePropositions)
                Problem.ResetProposition(p);
        }

        public class ValueProposition
        {
            public ValueProposition(FDVariable<T> var, T val)
            {
                variable = var;
                Value = val;
            }

            private readonly FDVariable<T> variable;
            public readonly T Value;

            public override string ToString()
            {
                return $"{variable.Name}={Value}";
            }
        }
    }
}
