#region Copyright
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a subclass of Pseudo Boolean Constraint in a Problem.
    /// A Conditional PBC is a PBC that has no effect in models for which the condition is false.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
#pragma warning disable 660,661
    internal class ConditionalPBC : PseudoBooleanConstraint
    {
        internal override string DebugName => $"{Condition} => {base.DebugName}";

        /// <summary>
        /// The condition Boolean in the clause that needs to be true
        /// If false, ignore the whole constraint.
        /// </summary>
        public readonly short Condition;

        // Include condition
        internal override IEnumerable<short> Literals
        {
            get
            {
                yield return Condition;
                foreach (var l in Disjuncts)
                    yield return l;
            }
        }

        /// <summary>
        /// Make a new clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="max">Maximum number of disjuncts that are allowed to be true to consider the clause satisfied, or 0 if the is no maximum</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        /// <param name="condition"> The condition literal in the clause that needs to be true </param>
        internal ConditionalPBC(ushort min, ushort max, short condition, short[] disjuncts) : base(min, max, disjuncts)
        {
            Condition = condition;
            IsConditional = true;
        }

        /// <summary>
        /// Check if the conditional PBC's condition literal is satisfied.
        /// </summary>
        public override bool IsEnabled(Solution s) => s.IsTrue(Condition);

        internal override IEnumerable<ushort> SpecialHandlingVariables()
        {
            yield return (ushort)Math.Abs(Condition);
        }

        public override void SpecialVariableFlipped(BooleanSolver b, ushort var, bool newValue)
        {
            // We've flipped our enabled state
            if (IsEnabled(b.Solution) && !IsSatisfied(b.TrueDisjunctCount[Index])
                // We need to check it isn't already there because if the condition is also in disjuncts,
                // then the normal flip handling may have already added it
                && !b.UnsatisfiedClauses.Contains(Index))
                // It's safe to add it
                b.UnsatisfiedClauses.Add(Index);
            // We were just disabled
            // Need to check if it's already been removed by normal flip handling, which can handle if the
            // condition is also one of the disjuncts.
            else if (b.UnsatisfiedClauses.Contains(Index))
                b.UnsatisfiedClauses.Remove(Index);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b)
        {
            var dCount = --b.TrueDisjunctCount[Index];
            var isEnabled = IsEnabled(b.Solution);
            if (b.UnsatisfiedClauses.Contains(Index))
            {
                if (OneTooManyDisjuncts((ushort) (dCount+1)) || !isEnabled)
                    // We just satisfied it or condition just disabled
                    b.UnsatisfiedClauses.Remove(Index);
            }
            // I think this needs to be !IsSatisfied rather than OneTooFewDisjuncts to handle
            // the case where the clause becomes enabled while it's satisfied by not at OneTooFewDisjuncts.
            else if (!IsSatisfied(dCount) && isEnabled)
                // It just transitioned from satisfied to unsatisfied, or condition just enabled
                b.UnsatisfiedClauses.Add(Index);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b)
        {
            var dCount = ++b.TrueDisjunctCount[Index];
            var isEnabled = IsEnabled(b.Solution);
            if (b.UnsatisfiedClauses.Contains(Index)) {
                if (OneTooFewDisjuncts((ushort)(dCount-1)) || !isEnabled)
                // We just satisfied it or it was disabled
                    b.UnsatisfiedClauses.Remove(Index);
            }
            // I think this needs to be !IsSatisfied rather than OneTooManyDisjuncts to handle
            // the case where the clause becomes enabled while it's satisfied by not at OneTooManyDisjuncts.
            else if (!IsSatisfied(dCount) && isEnabled)
                // It just transitioned from satisfied to unsatisfied, or condition just enabled
                b.UnsatisfiedClauses.Add(Index);
        }

        internal override void Decompile(Problem p, StringBuilder b)
        {
            b.Append(p.SATVariables[Math.Abs(Condition)].Proposition);
            b.Append(" => ");
            base.Decompile(p, b);
        }
    }
}
