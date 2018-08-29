#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Clause.cs" company="Ian Horswill">
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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a constraint in a Problem.
    /// The name is a slight misnomer, since a true clause is satisfied as long as at least one disjunct is satisfied.
    /// A clause in CatSAT is a generalized cardinality constraint, meaning the user can specify arbitrary max/min
    /// number of disjuncts may be satisfied.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
    class Clause
    {
        /// <summary>
        /// The literals of the clause
        /// </summary>
        internal readonly short[] Disjuncts;
        /// <summary>
        /// Minimum number of disjusts that must be true in order for the constraint
        /// to be satisfied, minus one.  For a normal clause, this is 0 (i.e. the real min number is 1)
        /// </summary>
        public readonly short MinDisjunctsMinusOne;
        /// <summary>
        /// Maximum number of disjucts that are allowed to be true in order for the
        /// constraint to be considered satisfied, plus one.
        /// For a normal clause, there is no limit, so this gets set to Disjuncts.Count+1.
        /// </summary>
        public readonly ushort MaxDisjunctsPlusOne;

        /// <summary>
        /// Position in the Problem's Clauses list.
        /// </summary>
        internal ushort Index;

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
        /// True if this is a plain old boring disjunction
        /// </summary>
        public bool IsNormalDisjunction => MinDisjunctsMinusOne == 0 && MaxDisjunctsPlusOne == Disjuncts.Length+1;

        /// <summary>
        /// Make a new clause (but doesn't add it to a Program)
        /// </summary>
        /// <param name="min">Minimum number of disjuncts that must be true to consider the clause satisfied</param>
        /// <param name="max">Maximum number of disjuncts that are allowed to be true to consider the clause satisfied, or 0 if the is no maximum</param>
        /// <param name="disjuncts">The disjuncts, encoded as signed proposition indices</param>
        internal Clause(ushort min, ushort max, short[] disjuncts)
        {
            Disjuncts = disjuncts.Distinct().ToArray();
            if ((min != 1 || max != 0) && disjuncts.Length != Disjuncts.Length)
                throw new ArgumentException("Nonstandard clause has non-unique disjuncts");
            MinDisjunctsMinusOne = (short)(min-1);
            MaxDisjunctsPlusOne = (ushort)(max == 0 ? disjuncts.Length+1 : max+1);
        }

        /// <summary>
        /// Return the number of disjuncts that are satisfied in the specified solution (i.e. model).
        /// </summary>
        /// <param name="solution">Solution to test against</param>
        /// <returns>Number of satisfied disjuncts</returns>
        public ushort CountDisjuncts(Solution solution)
        {
            ushort count = 0;
            foreach (var d in Disjuncts)
                if (solution.IsTrue(d))
                    count++;
            return count;
        }

        /// <summary>
        /// Is this constraint satisfied if the specified number of disjuncts is satisfied?
        /// </summary>
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <returns>Whether the constraint is satisfied.</returns>
        public bool IsSatisfied(ushort satisfiedDisjuncts)
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
    }
}
