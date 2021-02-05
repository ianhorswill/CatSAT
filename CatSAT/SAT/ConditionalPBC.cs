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
using System.Diagnostics;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a subclass of Pseudo Boolean Constraint in a Problem.
    /// A Conditional PBC is a PBC with an extra condition boolean.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
#pragma warning disable 660,661
    internal class ConditionalPBC : PseudoBooleanConstraint
    {
        /// <summary>
        /// The condition Boolean in the clause that needs to be true
        /// If false, ignore the whole constraint.
        /// </summary>
        public readonly short Condition;

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
        /// Is this constraint satisfied if the specified number of disjuncts is satisfied?
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <param name="solution">The solution object containing the model we're generating </param>
        /// <returns>Whether the constraint is satisfied.</returns>
        /// </summary>
        public override bool IsSatisfied(ushort satisfiedDisjuncts, Solution solution) 
            => solution.IsTrue(Condition) || base.IsSatisfied(satisfiedDisjuncts, solution);  //base.IsSatisfied(satisfiedDisjuncts)

        /// <summary>
        /// Check if the conditional PBC's condition literal is satisfied.
        /// </summary>
        public override bool IsEnabled(Solution s) => s.IsTrue(Condition);


        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b, Solution solution)
        {
            if (OneTooManyDisjuncts(b.TrueDisjunctCount[Index]))
                // We just satisfied it
                b.unsatisfiedClauses.Remove(Index);
            var dCount = --b.TrueDisjunctCount[Index];
            if (OneTooFewDisjuncts(dCount))
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(Index);
            //check if condition literal enabled
            if (IsEnabled(solution))
                b.unsatisfiedClauses.Add(Index);
            else 
                b.unsatisfiedClauses.Remove(Index);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b, Solution solution)
        {
            if (OneTooFewDisjuncts(b.TrueDisjunctCount[Index]))
                // We just satisfied it
                b.unsatisfiedClauses.Remove(Index);
            var dCount = ++b.TrueDisjunctCount[Index];
            if (OneTooManyDisjuncts(dCount))
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(Index);
            //check if condition literal enabled
            if (IsEnabled(solution))
                b.unsatisfiedClauses.Add(Index);
            else 
                b.unsatisfiedClauses.Remove(Index);
        }
    }
}
