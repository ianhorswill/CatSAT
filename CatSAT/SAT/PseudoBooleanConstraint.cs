#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constraint.cs" company="Ian Horswill">
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
    /// Represents a subclass of Constraint, for the pseudo-Boolean constraint, in a Problem.
    /// The name is a slight misnomer, since a true clause is satisfied as long as at least one disjunct is satisfied.
    /// A clause in CatSAT is a generalized cardinality constraint, meaning the user can specify arbitrary max/min
    /// number of disjuncts may be satisfied.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
#pragma warning disable 660,661
    internal class PseudoBooleanConstraint : Constraint
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
        /// Maximum number of disjuncts that are allowed to be true in order for the
        /// constraint to be considered satisfied, plus one.
        /// For a normal clause, there is no limit, so this gets set to Disjuncts.Count+1.
        /// </summary>
        public readonly ushort MaxDisjunctsPlusOne;

        /// <summary>
        /// Make a new clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="max">Maximum number of disjuncts that are allowed to be true to consider the clause satisfied, or 0 if the is no maximum</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        internal PseudoBooleanConstraint(ushort min, ushort max, short[] disjuncts) : base(min, disjuncts, min ^ max)
        {
            min = 1;
            if ((min != 1||max != 0) && disjuncts.Length != Disjuncts.Length)
                throw new ArgumentException("Nonstandard clause has non-unique disjuncts");
            MaxDisjunctsPlusOne = (ushort)(max == 0 ? disjuncts.Length + 1 : max + 1);
            IsNormalDisjunction = false; //MinDisjunctsMinusOne == 0 && MaxDisjunctsPlusOne >= Disjuncts.Length + 1;
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
        /// ThreatCountDelta when current clause is getting one more disjunct.
        /// </summary>
        public override int ThreatCountDeltaIncreasing(ushort count)
        {
            int threatCountDelta = 0;
            if (count == MinDisjunctsMinusOne) // Is the specified number of disjuncts one too few for this constraint to be satisfied?
                threatCountDelta = -1;
            else if ((ushort)(count + 1) == MaxDisjunctsPlusOne) // Is the specified number of disjuncts one too many for this constraint to be satisfied?
                threatCountDelta = 1;
            return threatCountDelta;
        }

        /// <summary>
        /// ThreatCountDelta when current clause is getting one less disjunct.
        /// </summary>
        public override int ThreatCountDeltaDecreasing(ushort count)
        {
            int threatCountDelta = 0;
            if ((ushort)(count - 1) == MinDisjunctsMinusOne) //Is the specified number of disjuncts one too few for this Clause to be satisfied? 
                threatCountDelta = 1;
            else if (count == MaxDisjunctsPlusOne) // Is the specified number of disjuncts one too many for this constraint to be satisfied?
                threatCountDelta = -1;
            return threatCountDelta;
        }
        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public override void UpdateTruePositiveAndFalseNegative(BooleanSolver b, ushort cIndex)
        {
            if (b.TrueDisjunctCount[cIndex] == MaxDisjunctsPlusOne) // Is the specified number of disjuncts one too many for this constraint to be satisfied?
                // We just satisfied it
                b.unsatisfiedClauses.Remove(cIndex);
            var dCount = --b.TrueDisjunctCount[cIndex];
            if (dCount == MinDisjunctsMinusOne)  //Is the specified number of disjuncts one too few for this Clause to be satisfied? 
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(cIndex);
        }
        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public override void UpdateTrueNegativeAndFalsePositive(BooleanSolver b, ushort cIndex)
        {
            if (b.TrueDisjunctCount[cIndex] == MinDisjunctsMinusOne) //Is the specified number of disjuncts one too few for this Clause to be satisfied? 
                // We just satisfied it
                b.unsatisfiedClauses.Remove(cIndex);
            var dCount = ++b.TrueDisjunctCount[cIndex];
            if (dCount == MaxDisjunctsPlusOne)
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(cIndex);
        }
        public string Decompile(Problem problem)
        {
            var b = new StringBuilder();
            var firstOne = true;
            foreach (var d in Disjuncts)
            {
                if (firstOne)
                    firstOne = false;
                else
                    b.Append(" | ");
                if (d < 0)
                    b.Append('!');
                b.Append(problem.SATVariables[Math.Abs(d)].Proposition.Name);
            }
            return b.ToString();
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
        /// Find the proposition from the specified clause that will do the least damage to the clauses that are already satisfied.
        /// </summary>
        /// <param name="b">Current BooleanSolver</param>
        /// <returns>Index of the prop to flip</returns>
        public override ushort GreedyFlip(BooleanSolver b)
        {
            // If true, the clause has too few disjuncts true
            bool increaseTrueDisjuncts = b.TrueDisjunctCount[Index] <= MinDisjunctsMinusOne;
            //Signed indices of the disjuncts of the clause
            short[] disjuncts = Disjuncts;
            //Variable that was last chosen for flipping in this clause
            ushort lastFlipOfThisClause = b.LastFlip[Index];


            var bestCount = int.MaxValue;
            var best = 0;

            //Walk disjuncts in a reasonably random order
            var dCount = (uint)disjuncts.Length;
            var index = Random.InRange(dCount);
            uint prime;
            do prime = Random.Prime(); while (prime <= dCount);
            for (var i = 0; i < dCount; i++)
            {
                var value = disjuncts[index];
                index = (index + prime) % dCount;
                var selectedVar = (ushort)Math.Abs(value);
                var truth = b.Propositions[selectedVar];
                if (value < 0) truth = !truth;
                if (truth == increaseTrueDisjuncts)
                    // This is already the right polarity
                    continue;

                if (selectedVar == lastFlipOfThisClause)
                    continue;
                var threatCount = b.UnsatisfiedClauseDelta(selectedVar);
                if (threatCount <= 0)
                    // Fast path - we've found an improvement; take it
                    // Real WalkSAT would continue searching for the best possible choice, but this
                    // gives better performance in my tests
                    // TODO - see if a faster way of computing ThreatenedClauseCount would improve things.
                    return selectedVar;

                if (threatCount < bestCount)
                {
                    best = selectedVar;
                    bestCount = threatCount;
                }
            }

            if (best == 0)
                return (ushort)Math.Abs(disjuncts.RandomElement());
            return (ushort)best;
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
