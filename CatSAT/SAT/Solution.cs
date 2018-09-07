#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Solution.cs" company="Ian Horswill">
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
#define RANDOMIZE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// The output of a program; a model satisfying the clauses of the Problem.
    /// Note: for packaging reasons, this is also where the actual solver code lives,
    /// rather than in Program.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Model) + "}")]
    public class Solution
    {
        #region Solver parameters
        /// <summary>
        /// Number of flips of propositions we can try before we give up and start over.
        /// </summary>
        public readonly int Timeout;
        #endregion

        #region Performance statistics
#if PerformanceStatistics
        /// <summary>
        /// Time taken to generate this solution
        /// </summary>
        public float SolveTimeMicroseconds { get; private set; }
        /// <summary>
        /// Number of flips required to generate this solution
        /// </summary>
        public int SolveFlips { get; private set; }
#endif
#endregion

        #region Solver state
        /// <summary>
        /// The Program for which this is a solution.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// States of the different propositions of the Program, indexed by proposition number.
        /// </summary>
        private readonly bool[] propositions;

        /// <summary>
        /// Number of presently true disjuncts in each of the Program's clauses, index by clause number.
        /// </summary>
        private readonly ushort[] trueDisjunctCount;

        /// <summary>
        /// Last flipped disjunct of a given clause
        /// </summary>
        private readonly ushort[] lastFlip;

        /// <summary>
        /// Total number of unsatisfied clauses
        /// </summary>
        private readonly List<ushort> unsatisfiedClauses = new List<ushort>();
        #endregion

        internal Solution(Problem problem, int timeout)
        {
            Problem = problem;
            Timeout = timeout;
            propositions = new bool[problem.SATVariables.Count];
            trueDisjunctCount = new ushort[problem.Clauses.Count];
            lastFlip = new ushort[problem.Clauses.Count];
        }

        /// <summary>
        /// A string listing the true propositions in the solution
        /// </summary>
        public string Model
        {
            // ReSharper disable once UnusedMember.Local
            get
            {
                var b = new StringBuilder();
                var firstOne = true;
                b.Append("{");
                for (int i = 1; i < propositions.Length; i++)
                {
                    if (propositions[i] && !Problem.SATVariables[i].Proposition.IsInternal)
                    {
                        if (firstOne)
                            firstOne = false;
                        else
                            b.Append(", ");
                        b.Append(Problem.SATVariables[i].Proposition);
                    }
                }

                foreach (var v in Problem.Variables())
                {
                    if (v.IsDefinedIn(this))
                    {
                        if (firstOne)
                            firstOne = false;
                        else
                            b.Append(", ");
                        b.Append(v.ValueString(this));
                    }
                }
                b.Append("}");
                return b.ToString();
            }
        }

        /// <summary>
        /// A string listing the performance statistics of the solver run that generated this solution.
        /// </summary>
        public string PerformanceStatistics
        {
            get
            {
#if PerformanceStatistics
                return $"{SolveTimeMicroseconds:#,##0.##}us {SolveFlips} flips, {SolveFlips/SolveTimeMicroseconds:0.##}MF/S";
#else
                return "CatSAT build does not have performance monitoring enabled.";
#endif
            }
        }

#region Checking truth values
        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool this[Literal l] => IsTrue(l);

        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        public bool this[Proposition p] => IsTrue(p);

        /// <summary>
        /// Test the truth of a literal (positive or negative) in the model.
        /// </summary>
        /// <param name="literal">Index of the literal (negative value = negative literal)</param>
        /// <returns>True if the literal is true in the model</returns>
        public bool IsTrue(short literal)
        {
            Debug.Assert(literal != 0, "0 is not a valid literal value!");
            if (literal > 0)
                return propositions[literal];
            return !propositions[-literal];
        }

        /// <summary>
        /// Test the truth of a proposition/positive literal
        /// </summary>
        /// <param name="index">Index of the proposition</param>
        /// <returns>True if the proposition is true in the model</returns>
        public bool IsTrue(ushort index)
        {
            return propositions[index];
        }

        /// <summary>
        /// Test the truth of the specified proposition within the model
        /// </summary>
        public bool IsTrue(Proposition p)
        {
            return IsTrue(p.Index);
        }

        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        public bool IsTrue(Literal l)
        {
            switch (l)
            {
                case Proposition p:
                    return IsTrue(p);

                case Negation n:
                    return !IsTrue(n.Proposition);

                default:
                    throw new ArgumentException($"Internal error - invalid literal {l}");
            }
        }
        #endregion

        #region Quantifiers
        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function to map domain element to proposition</param>
        /// <typeparam name="T">Element type of domain</typeparam>
        /// <returns>True if the right number of elements are true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Quantify<T>(int min, int max, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Quantify(min, max, domain.Select(f));
        }

        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if the right number of elements are true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Quantify(int min, int max, params Literal[] literals)
        {
            return Quantify(min, max, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if the right number of elements are true in this solution</returns>
        public bool Quantify(int min, int max, IEnumerable<Literal> literals)
        {
            var enumerable = literals as Literal[] ?? literals.ToArray();
            if (max == 0)
            {
                max = enumerable.Length;
            }
            var c = Count(enumerable);
            return c >= min && c <= max;
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>Number of literals that are true</returns>
        public int Count(params Literal[] literals)
        {
            return Count((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <returns>Number of literals that are true</returns>
        // ReSharper disable once UnusedMember.Global
        public int Count<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Count(domain.Select(f));
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>Number of literals that are true</returns>
        public int Count(IEnumerable<Literal> literals)
        {
            return literals.Count(IsTrue);
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if they're all true</returns>
        public bool All(params Literal[] literals)
        {
            return All((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <returns>True if they're all true</returns>
        public bool All<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return All(domain.Select(f));
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if they're all true</returns>
        public bool All(IEnumerable<Literal> literals)
        {
            var lits = literals.ToArray();
            return Quantify(lits.Length, lits.Length, (IEnumerable<Literal>)lits);
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists(params Literal[] literals)
        {
            return Exists((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Exists(domain.Select(f));
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists(IEnumerable<Literal> literals)
        {
            return literals.Any(IsTrue);
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Unique(params Literal[] literals)
        {
            return Unique((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <returns>True if exactly one literal is true in this solution</returns>
        public bool Unique<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Unique(domain.Select(f));
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly one literal is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Unique(IEnumerable<Literal> literals)
        {
            return Quantify(1, 1, literals);
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exactly(int n, params Literal[] literals)
        {
            return Exactly(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Maps a domain element to a literal</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exactly<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Exactly(n, domain.Select(f));
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Exactly(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, n, literals);
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtMost(int n, params Literal[] literals)
        {
            return AtMost(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Function to map a domain element to a literal</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtMost<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return AtMost(n, domain.Select(f));
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool AtMost(int n, IEnumerable<Literal> literals)
        {
            return Quantify(0, n, literals);
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast(int n, params Literal[] literals)
        {
            return AtLeast(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Function to map a domain element to a literal</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return AtLeast(n, domain.Select(f));
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, 0, literals);
        }
        #endregion

        #region Variables
        /// <summary>
        /// Get untyped value of variable
        /// </summary>
        public object this[Variable v] => v.UntypedValue(this);
        #endregion

        #region Solver
        private const int Theta = 3;
        private const float Phi = 0.2f;

        /// <summary>
        /// Try to find an assignment of truth values to propositions that satisfied the Program.
        /// Implements the WalkSAT algorithm
        /// </summary>
        /// <returns>True if a satisfying assignment was found.</returns>
        internal bool Solve()
        {
            if (propositions.Length == 1)
                // Trivial problem, since propositions[0] isn't a real proposition.
                return true;

#if PerformanceStatistics
            Problem.Stopwatch.Reset();
            Problem.Stopwatch.Start();
#endif

            var remainingFlips = Timeout;

            restart:
            MakeRandomAssignment();
            var flipsSinceImprovement = 0;
            var wp = 0f;

            for (; unsatisfiedClauses.Count > 0 && remainingFlips > 0; remainingFlips--)
            {
                // Hill climb: pick an unsatisfied clause at random and flip one of its variables
                var targetClauseIndex = unsatisfiedClauses.RandomElement();
                var targetClause = Problem.Clauses[targetClauseIndex];
                ushort flipChoice;

                if (Random.InRange(100) < 100 * wp)
                    // Flip a completely random variable
                    // This is to pull us out of local minima
                    flipChoice = (ushort) Math.Abs(targetClause.Disjuncts.RandomElement());
                else
                    // Hill climb: pick an unsatisfied clause at random and flip one of its variables;
                    flipChoice = GreedyFlip(TooFewTrue(targetClauseIndex), targetClause.Disjuncts, lastFlip[targetClauseIndex]);
                lastFlip[targetClauseIndex] = flipChoice;
                var oldSatisfactionCount = unsatisfiedClauses.Count;
                Flip(flipChoice);
                if (unsatisfiedClauses.Count < oldSatisfactionCount)
                {
                    // Improvement
                    flipsSinceImprovement = 0;
                    wp = wp * (1 - Phi / 2);
                }
                else
                {
                    flipsSinceImprovement++;
                    if (flipsSinceImprovement > Problem.Clauses.Count / Theta)
                    {
                        wp = wp + (1 - wp) * Phi;
                        flipsSinceImprovement = 0;
                    }
                }
            }

            if (unsatisfiedClauses.Count == 0 && Problem.TheorySolvers != null)
                // Ask the theory solvers, if any, to do their work
                if (!Problem.TheorySolvers.All(p => p.Value.Solve(this)))
                    // They failed; try again
                    goto restart;

#if PerformanceStatistics
            SolveTimeMicroseconds = Problem.Stopwatch.ElapsedTicks / (Stopwatch.Frequency * 0.000001f);
            SolveFlips = Timeout - remainingFlips;
#endif

            return unsatisfiedClauses.Count == 0;
        }

        private bool TooFewTrue(ushort targetClauseIndex)
        {
            return trueDisjunctCount[targetClauseIndex] <= Problem.Clauses[targetClauseIndex].MinDisjunctsMinusOne;
        }

        /// <summary>
        /// Find the proposition from the specified clause that will do the least damage to the clauses that are already satisfied.
        /// </summary>
        /// <param name="increaseTrueDisjuncts">If true, the clause has too few disjuncts true</param>
        /// <param name="disjuncts">Signed indices of the disjucts of the clause</param>
        /// <param name="lastFlipOfThisClause">Variable that was last chosen for flipping in this clause.</param>
        /// <returns>Index of the prop to flip</returns>
        private ushort GreedyFlip(bool increaseTrueDisjuncts, short[] disjuncts, ushort lastFlipOfThisClause)
        {
            var bestCount = int.MaxValue;
            var best = 0;
#if RANDOMIZE
            // Walk disjuncts in a reasonably random order
            var dCount = (uint)disjuncts.Length;
            var index = Random.InRange(dCount);
            var prime = Random.Prime();
            for (var i = 0; i < dCount; i++)
            {
                var value = disjuncts[index];
                index = (index + prime) % dCount;
#else
                foreach (var value in disjuncts)
            {
#endif
                var selectedVar = (ushort)Math.Abs(value);
                var truth = propositions[selectedVar];
                if (value < 0) truth = !truth;
                if (truth == increaseTrueDisjuncts)
                    // This is already the right polarity
                    continue;

                if (selectedVar == lastFlipOfThisClause)
                    continue;
                var threatCount = UnsatisfiedClauseDelta(selectedVar);
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

        /// <summary>
        /// The increase in the number of unsatisfied clauses as a result of flipping the specified variable
        /// </summary>
        /// <param name="pIndex">Index of the variable to consider flipping</param>
        /// <returns>The signed increase in the number of unsatisfied clauses</returns>
        int UnsatisfiedClauseDelta(ushort pIndex)
        {
            int threatCount = 0;
            var prop = Problem.SATVariables[pIndex];
            List<ushort> increasingClauses;
            List<ushort> decreasingClauses;

            if (propositions[pIndex])
            {
                // prop true -> false
                increasingClauses = prop.NegativeClauses;
                decreasingClauses = prop.PositiveClauses;
            }
            else
            {
                // prop true -> false
                increasingClauses = prop.PositiveClauses;
                decreasingClauses = prop.NegativeClauses;
            }

            foreach (var cIndex in increasingClauses)
            {
                // This clause is getting one more disjunct
                var clause = Problem.Clauses[cIndex];
                var count = trueDisjunctCount[cIndex];
                if (clause.OneTooFewDisjuncts(count))
                    threatCount--;
                else if (clause.OneTooManyDisjuncts((ushort) (count + 1)))
                    threatCount++;
            }

            foreach (var cIndex in decreasingClauses)
            {
                // This clause is getting one more disjunct
                var clause = Problem.Clauses[cIndex];
                var count = trueDisjunctCount[cIndex];
                if (clause.OneTooFewDisjuncts((ushort)(count-1)))
                    threatCount++;
                else if (clause.OneTooManyDisjuncts(count))
                    threatCount--;
            }

            

            //if (propositions[pIndex])
            //{
            //    // prop is currently true, so we would be flipping it to false

            //    // For positive literals the satisfied disjunct count would decrease
            //    foreach (var cIndex in prop.PositiveClauses)
            //    {
            //        if (Problem.Clauses[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
            //            threatCount++;
            //    }

            //    // For negative literals, the satisfied disjunct count would increase
            //    foreach (var cIndex in prop.NegativeClauses)
            //    {
            //        if (Problem.Clauses[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
            //            threatCount++;
            //    }
            //}
            //else
            //{
            //    // prop is currently false, so we would be flipping it to true

            //    // For positive literals the satisfied disjunct count would increase
            //    foreach (var cIndex in prop.PositiveClauses)
            //    {
            //        if (Problem.Clauses[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
            //            threatCount++;
            //    }

            //    // For negative literals, the satisfied disjunct count would decrease
            //    foreach (var cIndex in prop.NegativeClauses)
            //    {
            //        if (Problem.Clauses[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
            //            threatCount++;
            //    }
            //}

            return threatCount;
        }

        /// <summary>
        /// Flip the variable at the specified index.
        /// </summary>
        /// <param name="pIndex">Index of the variable/proposition to flip</param>
        private void Flip(ushort pIndex)
        {
            var prop = Problem.SATVariables[pIndex];
            if (prop.IsPredetermined)
                // Can't flip it.
                return;

            if (propositions[pIndex])
            {
                // Flip true -> false
                propositions[pIndex] = false;

                // Update the clauses in which this appears as a postive literal
                foreach (ushort cIndex in prop.PositiveClauses)
                {
                    // prop appears as a positive literal in clause.
                    // We just made it false, so clause now has fewer satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooManyDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = --trueDisjunctCount[cIndex];
                    if (clause.OneTooFewDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }

                // Update the clauses in which this appears as a negative literal
                foreach (ushort cIndex in prop.NegativeClauses)
                {
                    // prop appears as a negative literal in clause.
                    // We just made it false, so clause now has more satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooFewDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = ++trueDisjunctCount[cIndex];
                    if (clause.OneTooManyDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }
            }
            else
            {
                // Flip false -> true
                propositions[pIndex] = true;

                // Update the clauses in which this appears as a postive literal
                foreach (ushort cIndex in prop.PositiveClauses)
                {
                    // prop appears as a positive literal in clause.
                    // We just made it true, so clause now has more satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooFewDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = ++trueDisjunctCount[cIndex];
                    if (clause.OneTooManyDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }

                // Update the clauses in which this appears as a negative literal
                foreach (ushort cIndex in prop.NegativeClauses)
                {
                    // prop appears as a negative literal in clause.
                    // We just made it true, so clause now has fewer satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooManyDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = --trueDisjunctCount[cIndex];
                    if (clause.OneTooFewDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }
            }
        }

        /// <summary>
        /// Randomly assign values to the propositions,
        /// and initialize the other state information accordingly.
        /// </summary>
        private void MakeRandomAssignment()
        {
            // Initialize propositions[]
            for (var i = 0; i < propositions.Length; i++)
            {
                propositions[i] = Problem.SATVariables[i].IsPredetermined?Problem.SATVariables[i].PredeterminedValue:Problem.SATVariables[i].RandomInitialState;
            }

            unsatisfiedClauses.Clear();

            // Initialize trueDisjunctCount[] and unsatisfiedClauses
            for (ushort i = 0; i < trueDisjunctCount.Length; i++)
            {
                var c = Problem.Clauses[i];
                var satisfiedDisjuncts = c.CountDisjuncts(this);
                trueDisjunctCount[i] = satisfiedDisjuncts;
                if (!c.IsSatisfied(satisfiedDisjuncts))
                    unsatisfiedClauses.Add(i);
            }
        }
#endregion
    }
}
