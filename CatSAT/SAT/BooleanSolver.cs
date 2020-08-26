#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Solution.cs" company="Ian Horswill">
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
#define RANDOMIZE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CatSAT
{
    /// <summary>
    /// The output of a program; a model satisfying the clauses of the Problem.
    /// Note: for packaging reasons, this is also where the actual solver code lives,
    /// rather than in Program.
    /// </summary>
    public class BooleanSolver
    {
        /// <summary>
        /// The Program for which this is a solution.
        /// </summary>
        public readonly Problem Problem;

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
        /// The solution object containing the model we're generating.
        /// </summary>
        private Solution solution;

        /// <summary>
        /// States of the different propositions of the Program, indexed by proposition number.
        /// IMPORTANT: this is a cache of solution.Propositions.
        /// </summary>
        public bool[] propositions;

        /// <summary>
        /// Number of presently true disjuncts in each of the Program's clauses, index by clause number.
        /// </summary>
        public readonly ushort[] trueDisjunctCount;

        /// <summary>
        /// Last flipped disjunct of a given clause
        /// </summary>
        public readonly ushort[] lastFlip;

        /// <summary>
        /// Total number of unsatisfied clauses
        /// </summary>
        private DynamicUShortSet unsatisfiedClauses;

        /// <summary>
        /// Propositions that would increase utility if they were flipped.
        /// </summary>
        private DynamicUShortSet improvablePropositions;

        /// <summary>
        /// The total utility of all the true propositions in the solution.
        /// </summary>
        private float totalUtility;

        #endregion

        internal BooleanSolver(Problem problem)
        {
            Problem = problem;
            var clausesCount = problem.Clauses.Count;
            trueDisjunctCount = new ushort[clausesCount];
            lastFlip = new ushort[clausesCount];
            unsatisfiedClauses = new DynamicUShortSet(clausesCount);
            improvablePropositions = new DynamicUShortSet(problem.SATVariables.Count);

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

        #region Solver
        private const int Theta = 3;
        private const float Phi = 0.2f;

        /// <summary>
        /// Try to find an assignment of truth values to propositions that satisfied the Program.
        /// Implements the WalkSAT algorithm
        /// </summary>
        /// <returns>True if a satisfying assignment was found.</returns>
        internal bool Solve(Solution s, int timeout, out int unusedFlips, bool continuePreviousSearch = false)
        {
            solution = s;
            propositions = s.Propositions;
            if (propositions.Length == 1 && Problem.TheorySolvers == null)
                // Trivial problem, since propositions[0] isn't a real proposition.
            {
                unusedFlips = timeout;
                return true;
            }

#if PerformanceStatistics
            Problem.Stopwatch.Reset();
            Problem.Stopwatch.Start();
#endif

            var remainingFlips = timeout;

            s.Problem.ResetPreinitialization(); 
            s.Problem.InvokeInitialization();

        restart:
            // Make a random assignment, unless we've been specifically told to continue from where we were
            if (continuePreviousSearch)
                continuePreviousSearch = false;
            else
                MakeRandomAssignment();

            var flipsSinceImprovement = 0;
            var wp = 0f;

            for (; unsatisfiedClauses.Size > 0 && remainingFlips > 0; remainingFlips--)
            {
                // Hill climb: pick an unsatisfied clause at random and flip one of its variables
                var targetClauseIndex = unsatisfiedClauses.RandomElement;
                var targetClause = Problem.Clauses[targetClauseIndex];
                ushort flipChoice;

                if (Random.InRange(100) < 100 * wp)
                    // Flip a completely random variable
                    // This is to pull us out of local minima
                    flipChoice = (ushort) Math.Abs(targetClause.Disjuncts.RandomElement());
                else
                    // Hill climb: pick an unsatisfied clause at random and flip one of its variables;
                    flipChoice = targetClause.GreedyFlip(this);
                lastFlip[targetClauseIndex] = flipChoice;
                var oldSatisfactionCount = unsatisfiedClauses.Size;
                Flip(flipChoice);
                //do NOT update noise level if it's a Pseudo Boolean Constraint (i.e. not normal)
                if (targetClause.IsNormalDisjunction)
                {
                    if (unsatisfiedClauses.Size < oldSatisfactionCount)
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
            }

            if (unsatisfiedClauses.Size == 0 && Problem.TheorySolvers != null)
                // Ask the theory solvers, if any, to do their work
                if (!Problem.TheorySolvers.All(p => p.Value.Solve(solution)))
                    // They failed; try again
                    goto restart;

#if PerformanceStatistics
            SolveTimeMicroseconds = Problem.Stopwatch.ElapsedTicks / (Stopwatch.Frequency * 0.000001f);
            SolveFlips = timeout - remainingFlips;
#endif
            solution.Utility = totalUtility;
            unusedFlips = remainingFlips-1;
            return unsatisfiedClauses.Size == 0;
        }

        private bool TooFewTrue(ushort targetClauseIndex)
        {
            return trueDisjunctCount[targetClauseIndex] <= Problem.Clauses[targetClauseIndex].MinDisjunctsMinusOne;
        }

        /// <summary>
        /// Find the proposition from the specified clause that will do the least damage to the clauses that are already satisfied.
        /// </summary>
        /// <param name="increaseTrueDisjuncts">If true, the clause has too few disjuncts true</param>
        /// <param name="disjuncts">Signed indices of the disjuncts of the clause</param>
        /// <param name="lastFlipOfThisClause">Variable that was last chosen for flipping in this clause.</param>
        /// <returns>Index of the prop to flip</returns>
        /*private ushort GreedyFlip(ushort targetClauseIndex)
        {
            var bestCount = int.MaxValue;
            var best = 0;
            bool increaseTrueDisjuncts = TooFewTrue(targetClauseIndex);
            short[] disjuncts = Problem.Clauses[targetClauseIndex].Disjuncts;

            ushort lastFlipOfThisClause = lastFlip[targetClauseIndex];
#if RANDOMIZE
            // Walk disjuncts in a reasonably random order
            var dCount = (uint)disjuncts.Length;
            var index = Random.InRange(dCount);
            uint prime;
            do prime = Random.Prime(); while (prime <= dCount);
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
        */

        /// <summary>
        /// The increase in the number of unsatisfied clauses as a result of flipping the specified variable
        /// </summary>
        /// <param name="pIndex">Index of the variable to consider flipping</param>
        /// <returns>The signed increase in the number of unsatisfied clauses</returns>
        public int UnsatisfiedClauseDelta(ushort pIndex)
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

            var currentlyTrue = propositions[pIndex];
            var utility = prop.Proposition.Utility;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (utility != 0)
            {
                if (utility < 0 ^ currentlyTrue)
                {
                    // This flip lowers utility
                    improvablePropositions.Add(pIndex);
                }
                else
                {
                    // This flip increases utility
                    improvablePropositions.Remove(pIndex);
                }
            }

            {
                if (currentlyTrue)
                {
                    // Flip true -> false
                    propositions[pIndex] = false;
                    totalUtility -= Problem.SATVariables[pIndex].Proposition.Utility;

                    // Update the clauses in which this appears as a positive literal
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
                    totalUtility += Problem.SATVariables[pIndex].Proposition.Utility;

                    // Update the clauses in which this appears as a positive literal
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
            //CheckUtility();
        }

        /// <summary>
        /// Forcibly flip improvable proposition to increase utility, probably at the expense
        /// breaking a clause.
        /// TODO: Make this actually be efficient
        /// </summary>
        /// <returns>True if solution wasn't already the maximum possible utility</returns>
        internal bool ImproveUtility(int propCount)
        {
            if (improvablePropositions.Size == 0)
                return false;

            for (var i = 0; i < propCount && improvablePropositions.Size > 0; i++)
                // Randomly flip one
                Flip(improvablePropositions.RandomElement);
            return true;
        }

        /// <summary>
        /// Check the invariant that totalUtility == the sum of the utilities of all true propositions
        /// </summary>
        [Conditional("DEBUG")]
        // ReSharper disable once UnusedMember.Local
        private void CheckUtility()
        {
            #if DEBUG
            var sum = 0.0f;
            for (var i = 0; i < propositions.Length; i++)
                if (propositions[i])
                    sum += Problem.SATVariables[i].Proposition.Utility;
            Debug.Assert(Math.Abs(totalUtility - sum) < 0.0001f, "totalUtility incorrect");
            #endif
        }


        /// <summary>
        /// Randomly assign values to the propositions,
        /// and initialize the other state information accordingly.
        /// </summary>
        private void MakeRandomAssignment()
        {
            totalUtility = 0;
            improvablePropositions.Clear();
            var vars = Problem.SATVariables;

            // Initialize propositions[] and compute totalUtility
            for (ushort i = 0; i < propositions.Length; i++)
            {
                var satVar = vars[i];
                var truth = satVar.IsPredetermined?satVar.PredeterminedValue:satVar.RandomInitialState;
                propositions[i] = truth;
                var utility = satVar.Proposition.Utility;
                if (truth ^ utility > 0)
                    improvablePropositions.Add(i);
                if (truth)
                    totalUtility += utility;
            }

            unsatisfiedClauses.Clear();

            // Initialize trueDisjunctCount[] and unsatisfiedClauses
            for (ushort i = 0; i < trueDisjunctCount.Length; i++)
            {
                var c = Problem.Clauses[i];
                var satisfiedDisjuncts = c.CountDisjuncts(solution);
                trueDisjunctCount[i] = satisfiedDisjuncts;
                if (!c.IsSatisfied(satisfiedDisjuncts))
                    unsatisfiedClauses.Add(i);
            }

            //CheckUtility();
        }
        #endregion

        #region Dynamic sets

        private struct DynamicUShortSet
        {
            public DynamicUShortSet(int max)
            {
                contents = new ushort[max];
                indices = new ushort[max];
                Size = 0;
                Clear();
            }

            private readonly ushort[] contents;
            private readonly ushort[] indices;
            public ushort Size;

            // ReSharper disable once UnusedMember.Local
            public ushort this[int i]
            {
                get => contents[i];
                set => contents[i] = value;
            }

            public ushort RandomElement => contents[Random.Next() % Size];

            public void Add(ushort elt)
            {
                var index = Size++;
                Debug.Assert(indices[elt] == ushort.MaxValue, "Adding element already in set");
                indices[elt] = index;
                contents[index] = elt;
            }

            public void Remove(ushort elt)
            {
                Debug.Assert(indices[elt] != ushort.MaxValue, "Deleting element not in set");
                // Replace elt in the contents array with the last element in the contents array
                var index = indices[elt];
                var replacement = contents[--Size];
                contents[index] = replacement;
                indices[replacement] = index;
                #if DEBUG
                indices[elt] = ushort.MaxValue;
                #endif
            }

            public void Clear()
            {
#if DEBUG
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = ushort.MaxValue;
#endif
                Size = 0;
            }
        }
        #endregion
    }
}
