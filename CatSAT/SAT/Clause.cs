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
    /// Represents a Clause in a Problem.
    /// The name is a slight misnomer, since a true clause is satisfied as long as at least one disjunct is satisfied.
    /// A clause in CatSAT is a generalized cardinality Clause, meaning the user can specify arbitrary min
    /// number of disjuncts may be satisfied.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
#pragma warning disable 660,661
    internal class Clause : Constraint
#pragma warning restore 660,661
    {
        internal string DebugName
        {
            get
            {
                var b = new StringBuilder();
                var firstOne = true;
                b.Append("<");
                foreach (var d in Disjuncts)
                {
                    if (firstOne)
                        firstOne = false;
                    else
                        b.Append(" ");
                    b.Append(d);
                }
                b.Append(">");
                return b.ToString();
            }
        }


        /// <summary>
        /// Make a new normal clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        internal Clause(short[] disjuncts) : base(true, 1, disjuncts, 1)
        { }

        public override int GetHashCode() => Hash;

        internal override bool EquivalentTo(Constraint c)
        {
            if (!(c is Clause normalConstraint))
                return false;
            if (Disjuncts.Length != normalConstraint.Disjuncts.Length)
                return false;
            for (var i = 0; i < Disjuncts.Length; i++)
                if (Disjuncts[i] != normalConstraint.Disjuncts[i])
                    return false;
            return true;
        }

        public static bool operator ==(Clause a, Clause b) => a?.EquivalentTo(b) ?? ReferenceEquals(b, null);

        public static bool operator !=(Clause a, Clause b) => !(a == b);


        /// <summary>
        /// Is this Clause satisfied if the specified number of disjuncts is satisfied?
        /// </summary>
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <returns>Whether the Clause is satisfied.</returns>
        public override bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts > 0;
        }

        /// <summary>
        /// ThreatCountDelta when current clause is getting one more true disjunct.
        /// </summary>
        public override int ThreatCountDeltaIncreasing(ushort count)
        {
            return (count == 0) ? -1 : 0;
        }
        /// <summary>
        /// ThreatCountDelta when current clause is getting one less true disjunct.
        /// </summary>
        public override int ThreatCountDeltaDecreasing(ushort count)
        {
            return (count == 1) ? 1 : 0;
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b)
        {
            var dCount = --b.TrueDisjunctCount[Index];
            if (dCount == 0)
                // It just transitioned from satisfied to unsatisfied
                b.UnsatisfiedClauses.Add(Index);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b)
        {
            if (b.TrueDisjunctCount[Index] == 0)
                // We just satisfied it
                b.UnsatisfiedClauses.Remove(Index);
            b.TrueDisjunctCount[Index]++;
        }
        /// <summary>
        /// Return the max number of false literals in a normal clause.
        /// </summary>
        public override bool MaxFalseLiterals(int falseLiterals)
        {
            return falseLiterals == Disjuncts.Length - 1;
        }
        /// <summary>
        /// Return the max number of true literals in a normal clause.
        /// </summary>
        public override bool MaxTrueLiterals(int trueLiterals)
        {
            return trueLiterals == Disjuncts.Length;
        }

        internal override void Decompile(Problem p, StringBuilder b)
        {
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

            //if (MaxDisjunctsPlusOne < Disjuncts.Length + 1)
                //b.Append($" {MaxDisjunctsPlusOne - 1}");
        }
    }
}
