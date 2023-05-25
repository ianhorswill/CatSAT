﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constraints.cs" company="Ian Horswill">
// Copyright (C) 2018, 2019 Ian Horswill
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
using System.Diagnostics;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a subclass of Constraint, for the pseudo-Boolean constraints, in a Problem.
    /// The name is a slight misnomer, since a true clause is satisfied as long as at least one disjunct is satisfied.
    /// A PseudoBoolean Constrain in CatSAT is a generalized cardinality constraint, meaning the user can specify arbitrary max/min
    /// number of disjuncts may be satisfied.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
#pragma warning disable 660,661
    internal class PseudoBooleanConstraint : Constraint
    {
        internal override string DebugName => $"{MinDisjunctsMinusOne + 1} {base.DebugName} {MaxDisjunctsPlusOne - 1}";

        /// <summary>
        /// Maximum number of disjuncts that are allowed to be true in order for the
        /// constraint to be considered satisfied, plus one.
        /// For a normal clause, there is no limit, so this gets set to Disjuncts.Count+1.
        /// </summary>
        public readonly ushort MaxDisjunctsPlusOne;

        /// <summary>
        /// True if this is a Conditional PBC
        /// Currently not referenced... yet
        /// </summary>
        public bool IsConditional { get; protected set; }

        /// <summary>
        /// Make a new clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="max">Maximum number of disjuncts that are allowed to be true to consider the clause satisfied, or 0 if the is no maximum</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        internal PseudoBooleanConstraint(ushort min, ushort max, short[] disjuncts) : base(false, min, disjuncts, min ^ max)
        {
            MaxDisjunctsPlusOne = (ushort)(max == 0 ? disjuncts.Length + 1 : max + 1);
        }

        /// <summary>
        /// Is this constraint satisfied if the specified number of disjuncts is satisfied?
        /// </summary>
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <returns>Whether the constraint is satisfied.</returns>
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts > MinDisjunctsMinusOne && satisfiedDisjuncts < MaxDisjunctsPlusOne;
        }

        /// <summary>
        /// Is the specified number of disjuncts one too many for this constraint to be satisfied?
        /// </summary>
        public bool OneTooManyDisjuncts(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts == MaxDisjunctsPlusOne;
        }

        /// <summary>
        /// Is the specified number of disjuncts one too few for this constraint to be satisfied?
        /// </summary>
        public bool OneTooFewDisjuncts(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts == MinDisjunctsMinusOne;
        }

        /// <summary>
        /// ThreatCountDelta when current clause is getting one more true disjunct.
        /// </summary>
        public override int ThreatCountDeltaIncreasing(ushort count)
        {
            int threatCountDelta = 0;
            if (OneTooFewDisjuncts(count))
                threatCountDelta = -1;
            else if (OneTooManyDisjuncts((ushort)(count + 1)))
                threatCountDelta = 1;
            return threatCountDelta;
        }
        /// <summary>
        /// ThreatCountDelta when current clause is getting one less true disjunct.
        /// </summary>
        public override int ThreatCountDeltaDecreasing(ushort count)
        {
            int threatCountDelta = 0;
            if (OneTooFewDisjuncts((ushort)(count - 1)))
                threatCountDelta = 1;
            else if (OneTooManyDisjuncts(count))
                threatCountDelta = -1;
            return threatCountDelta;
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b)
        {
            var dCount = --b.TrueDisjunctCount[Index];
            if (OneTooManyDisjuncts((ushort)(dCount+1)))
                // We just satisfied it
                b.UnsatisfiedClauses.Remove(Index);
            else if (OneTooFewDisjuncts(dCount))
                // It just transitioned from satisfied to unsatisfied
                b.UnsatisfiedClauses.Add(Index);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b)
        {
            var dCount = b.TrueDisjunctCount[Index]++;
            if (OneTooFewDisjuncts(dCount))
                // We just satisfied it
                b.UnsatisfiedClauses.Remove(Index);
            else if (OneTooManyDisjuncts((ushort) (dCount + 1)))
                // It just transitioned from satisfied to unsatisfied
                b.UnsatisfiedClauses.Add(Index);
        }

    

    public override int GetHashCode() => Hash;

        internal override bool EquivalentTo(Constraint c)
        {
            if (!(c is PseudoBooleanConstraint pbConstraint))
                return false;
            if (MaxDisjunctsPlusOne != pbConstraint.MaxDisjunctsPlusOne)
                return false;
            if (MinDisjunctsMinusOne != 0)
                return false;
            if (Disjuncts.Length != pbConstraint.Disjuncts.Length)
                return false;
            for (var i = 0; i < Disjuncts.Length; i++)
                if (Disjuncts[i] != pbConstraint.Disjuncts[i])
                    return false;
            return true;
        }

        public static bool operator ==(PseudoBooleanConstraint a, PseudoBooleanConstraint b) => a?.EquivalentTo(b) ?? ReferenceEquals(b, null);

        public static bool operator !=(PseudoBooleanConstraint a, PseudoBooleanConstraint b) => !(a == b);

        /// <summary>
        /// Return the max number of false literals in a PseudoBoolean constraint.
        /// </summary>
        public override bool MaxFalseLiterals(int falseLiterals)
        {
            return falseLiterals == (Disjuncts.Length - (MinDisjunctsMinusOne + 1));
        }
        /// <summary>
        /// Return the max number of true literals in a PseudoBoolean constraint.
        /// </summary>
        public override bool MaxTrueLiterals(int trueLiterals)
        {
            return trueLiterals == MaxDisjunctsPlusOne - 1;
        }

        public override void UpdateCustomConstraint(BooleanSolver b, ushort pIndex, bool newValue)
        {
            throw new NotImplementedException();
        }

        public override int CustomFlipRisk(ushort index, bool newValue)
        {
            throw new NotImplementedException();
        }

        internal override void Decompile(Problem p, StringBuilder b)
        {
            if (MinDisjunctsMinusOne != 0 || MaxDisjunctsPlusOne < Disjuncts.Length + 1)
                b.Append($"{MinDisjunctsMinusOne + 1} ");
            var firstLit = true;
            foreach (var d in Disjuncts)
            {
                if (firstLit)
                    firstLit = false;
                else
                    b.Append(" | ");
                if (d < 0)
                    b.Append("!");
                b.Append(p.SATVariables[Math.Abs(d)].Proposition);
            }

            if (MaxDisjunctsPlusOne < Disjuncts.Length + 1)
                b.Append($" {MaxDisjunctsPlusOne - 1}");
        }
    }
}
