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
        public readonly Boolean ConditionBoolean;

        /// <summary>
        /// The BooleanSolver for which this is a solution.
        /// </summary>
        public readonly BooleanSolver BooleanSolver;


        /// <summary>
        /// Make a new clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="max">Maximum number of disjuncts that are allowed to be true to consider the clause satisfied, or 0 if the is no maximum</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        /// <param name="conditionBoolean"> The condition Boolean in the clause that needs to be true </param>
        internal ConditionalPBC(ushort min, ushort max, bool conditionBoolean, short[] disjuncts) : base(min, max, disjuncts)
        {
            ConditionBoolean = conditionBoolean;
            IsConditional = true;
        }

        /// <summary>
        /// Return the number of disjuncts that are satisfied in the specified solution (i.e. model).
        ///  Currently not referenced... yet
        /// <param name="solution">Solution to test against</param>
        /// <returns>Number of satisfied disjuncts</returns>
        public override ushort CountDisjuncts(Solution solution)
        {
            ushort count = 0;
            if (ConditionBoolean)
                return (ushort) (MinDisjunctsMinusOne + 1);
            else 
                foreach (var d in Disjuncts) 
                    if (solution.IsTrue(d)) 
                        count++; 
            return count;
        }

        /// <summary>
        /// Is this constraint satisfied if the specified number of disjuncts is satisfied?
        /// Currently not referenced... yet
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <returns>Whether the constraint is satisfied.</returns>
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            if (ConditionBoolean)
                return true;
            else 
                return satisfiedDisjuncts > MinDisjunctsMinusOne && satisfiedDisjuncts < MaxDisjunctsPlusOne;
        }
    }
}
