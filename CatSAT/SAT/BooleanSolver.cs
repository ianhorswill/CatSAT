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
        public Solution Solution;

        /// <summary>
        /// States of the different propositions of the Program, indexed by proposition number.
        /// IMPORTANT: this is a cache of solution.Propositions.
        /// </summary>
        public bool[] Propositions;

        /// <summary>
        /// Number of presently true disjuncts in each of the Program's clauses, index by clause number.
        /// </summary>
        public readonly ushort[] TrueDisjunctCount;

        /// <summary>
        /// Last flipped disjunct of a given clause
        /// </summary>
        public readonly ushort[] LastFlip;

        /// <summary>
        /// Total number of unsatisfied clauses
        /// </summary>
        public DynamicUShortSet UnsatisfiedClauses;

        /// <summary>
        /// Propositions that would increase utility if they were flipped.
        /// </summary>
        private DynamicUShortSet improvablePropositions;

        /// <summary>
        /// The total utility of all the true propositions in the solution.
        /// </summary>
        private float totalUtility;

        /// <summary>
        /// Arrays to hold state information; indexed by clause number.
        /// </summary>
        private bool[] alreadySatisfied;
        private ushort[] trueLiterals;
        private ushort[] falseLiterals;

        /// <summary>
        /// Arrays to hold state information; indexed by proposition number.
        /// </summary>
        private bool[] varInitialized;
        #endregion

        internal BooleanSolver(Problem problem)
        {
            Problem = problem;
            var clausesCount = problem.Constraints.Count;
            TrueDisjunctCount = new ushort[clausesCount];
            LastFlip = new ushort[clausesCount];
            UnsatisfiedClauses = new DynamicUShortSet(clausesCount);
            improvablePropositions = new DynamicUShortSet(problem.SATVariables.Count);
            alreadySatisfied = new bool[Problem.Constraints.Count];
            falseLiterals = new ushort[Problem.Constraints.Count];
            trueLiterals = new ushort[Problem.Constraints.Count];
            varInitialized = new bool[Problem.SATVariables.Count];
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
            Solution = s;
            Propositions = s.Propositions;
            if (Propositions.Length == 1 && Problem.TheorySolvers == null)
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
            
            for (; UnsatisfiedClauses.Size > 0 && remainingFlips > 0; remainingFlips--)
            {
                // Hill climb: pick an unsatisfied clause at random and flip one of its variables
                var targetClauseIndex = UnsatisfiedClauses.RandomElement;
                var targetClause = Problem.Constraints[targetClauseIndex];
                ushort flipChoice;
                targetClause.UnPredeterminedDisjuncts = new List<short>();
                foreach (short lit in targetClause.Disjuncts)
                {
                    if (!Problem.SATVariables[(ushort)Math.Abs(lit)].IsPredetermined)
                        targetClause.UnPredeterminedDisjuncts.Add(lit);
                    else { targetClause.UnPredeterminedDisjuncts.Remove(lit); }
                }

                if (Random.InRange(100) < 100 * wp)
                    // Flip a completely random variable
                    // This is to pull us out of local minima
                    flipChoice = (ushort) Math.Abs(targetClause.UnPredeterminedDisjuncts.RandomElement());
                else
                    // Hill climb: pick an unsatisfied clause at random and flip one of its variables;
                    flipChoice = targetClause.GreedyFlip(this);
                LastFlip[targetClauseIndex] = flipChoice;
                var oldSatisfactionCount = UnsatisfiedClauses.Size;
                Flip(flipChoice);
                //do NOT update noise level if it's a Pseudo Boolean Constraint (i.e. not normal)
                if (targetClause.IsNormalDisjunction)
                {
                    if (UnsatisfiedClauses.Size < oldSatisfactionCount)
                    {
                        // Improvement
                        flipsSinceImprovement = 0;
                        wp = wp * (1 - Phi / 2);
                    }
                    else
                    {
                        flipsSinceImprovement++;
                        if (flipsSinceImprovement > Problem.Constraints.Count / Theta)
                        {
                            wp = wp + (1 - wp) * Phi;
                            flipsSinceImprovement = 0;
                        }
                    }
                }
            }

            if (UnsatisfiedClauses.Size == 0 && Problem.TheorySolvers != null)
                // Ask the theory solvers, if any, to do their work
                if (!Problem.TheorySolvers.All(p => p.Value.Solve(Solution)))
                    // They failed; try again
                    goto restart;

#if PerformanceStatistics
            SolveTimeMicroseconds = Problem.Stopwatch.ElapsedTicks / (Stopwatch.Frequency * 0.000001f);
            SolveFlips = timeout - remainingFlips;
#endif
            Solution.Utility = totalUtility;
            unusedFlips = remainingFlips-1;
            return UnsatisfiedClauses.Size == 0;
        }

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

            if (Propositions[pIndex])
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
                var clause = Problem.Constraints[cIndex];
                var count = TrueDisjunctCount[cIndex];
                threatCount += clause.ThreatCountDeltaIncreasing(count);
            }

            foreach (var cIndex in decreasingClauses)
            {
                // This clause is getting one more disjunct
                var clause = Problem.Constraints[cIndex];
                var count = TrueDisjunctCount[cIndex];
                threatCount += clause.ThreatCountDeltaDecreasing(count);
            }



            //if (propositions[pIndex])
            //{
            //    // prop is currently true, so we would be flipping it to false

            //    // For positive literals the satisfied disjunct count would decrease
            //    foreach (var cIndex in prop.PositiveClauses)
            //    {
            //        if (Problem.Constraint[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
            //            threatCount++;
            //    }

            //    // For negative literals, the satisfied disjunct count would increase
            //    foreach (var cIndex in prop.NegativeClauses)
            //    {
            //        if (Problem.Constraint[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
            //            threatCount++;
            //    }
            //}
            //else
            //{
            //    // prop is currently false, so we would be flipping it to true

            //    // For positive literals the satisfied disjunct count would increase
            //    foreach (var cIndex in prop.PositiveClauses)
            //    {
            //        if (Problem.Constraint[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
            //            threatCount++;
            //    }

            //    // For negative literals, the satisfied disjunct count would decrease
            //    foreach (var cIndex in prop.NegativeClauses)
            //    {
            //        if (Problem.Constraint[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
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
            Debug.Assert(!prop.IsPredetermined, "The prop is predetermined, can't flip it.");
            var currentlyTrue = Propositions[pIndex];
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
                    Propositions[pIndex] = false;
                    totalUtility -= Problem.SATVariables[pIndex].Proposition.Utility;

                    // Update the clauses in which this appears as a positive literal
                    foreach (ushort cIndex in prop.PositiveClauses)
                    {
                        // prop appears as a positive literal in clause.
                        // We just made it false, so clause now has fewer satisfied disjuncts.
                        var clause = Problem.Constraints[cIndex];
                        clause.UpdateTruePositiveAndFalseNegative(this);
                    }

                    // Update the clauses in which this appears as a negative literal
                    foreach (ushort cIndex in prop.NegativeClauses)
                    {
                        // prop appears as a negative literal in clause.
                        // We just made it false, so clause now has more satisfied disjuncts.
                        var clause = Problem.Constraints[cIndex];
                        clause.UpdateTrueNegativeAndFalsePositive(this);
                    }
                }
                else
                {
                    // Flip false -> true
                    Propositions[pIndex] = true;
                    totalUtility += Problem.SATVariables[pIndex].Proposition.Utility;

                    // Update the clauses in which this appears as a positive literal
                    foreach (ushort cIndex in prop.PositiveClauses)
                    {
                        // prop appears as a positive literal in clause.
                        // We just made it true, so clause now has more satisfied disjuncts.
                        var clause = Problem.Constraints[cIndex];
                        clause.UpdateTrueNegativeAndFalsePositive(this);
                    }

                    // Update the clauses in which this appears as a negative literal
                    foreach (ushort cIndex in prop.NegativeClauses)
                    {
                        // prop appears as a negative literal in clause.
                        // We just made it true, so clause now has fewer satisfied disjuncts.
                        var clause = Problem.Constraints[cIndex];
                        clause.UpdateTruePositiveAndFalseNegative(this);
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
            for (var i = 0; i < Propositions.Length; i++)
                if (Propositions[i])
                    sum += Problem.SATVariables[i].Proposition.Utility;
            Debug.Assert(Math.Abs(totalUtility - sum) < 0.0001f, "totalUtility incorrect");
            #endif
        }

        /// <summary>
        /// update the utility of the current initializing proposition.
        /// <param name="pIndex">Index of the variable/proposition to be updated</param>
        /// </summary>
        private void UpdateUtility(ushort pIndex)
        {
            var satVar = Problem.SATVariables[pIndex];
            var currentType = Propositions[pIndex];
            var utility = satVar.Proposition.Utility;
            if (currentType ^ utility > 0)
                improvablePropositions.Add(pIndex);
            if (currentType)
                totalUtility += utility;
        }


        /// <summary>
        /// propagate a proposition
        /// <param name="pIndex">Index of the variable/proposition to be propagated</param>
        /// <param name="initialValue">the truth value will be assigned to proposition pIndex</param>
        /// </summary>
        private void Propagate(ushort pIndex, bool initialValue)
        {
            var satVar = Problem.SATVariables[pIndex];
            Propositions[pIndex] = initialValue;
            varInitialized[pIndex] = true;
            UpdateUtility(pIndex);

            var increasedClauses = initialValue ? satVar.PositiveClauses : satVar.NegativeClauses;
            var notIncreasedClauses = initialValue ? satVar.NegativeClauses : satVar.PositiveClauses;
            
            foreach (var cIndex in increasedClauses)
            {
                var clause = Problem.Constraints[cIndex];
                trueLiterals[cIndex]++;
                if (clause.IsNormalDisjunction && clause.IsSatisfied(trueLiterals[cIndex]))
                    alreadySatisfied[cIndex] = true;
                else if (clause.MaxTrueLiterals(trueLiterals[cIndex]))// if we get to max true literals
                {
                    alreadySatisfied[clause.Index] = true;
                    foreach (var lit in clause.Disjuncts)
                    {
                        var prop = (ushort)Math.Abs(lit);
                        if (!varInitialized[prop] && !Problem.SATVariables[prop].IsPredetermined)
                        {
                            // Found the last one uninitialized variable; make sure it's false
                            Propagate(prop, lit < 0);
                            // Don't need to look for further disjuncts
                            break;
                        }
                        
                    }
                }
            }

            foreach (var cIndex in notIncreasedClauses)
            {
                var clause = Problem.Constraints[cIndex];
                if (!alreadySatisfied[cIndex]) 
                {
                    falseLiterals[cIndex]++;
                    if (clause.MaxFalseLiterals(falseLiterals[cIndex]))// if we get to max false literals
                    {
                        alreadySatisfied[clause.Index] = true;
                        foreach (var lit in clause.Disjuncts)
                        {
                            var prop = (ushort)Math.Abs(lit);
                            if (!varInitialized[prop] && !Problem.SATVariables[prop].IsPredetermined)
                            {
                                // Found the one uninitialized variable; make sure it's true
                                Propagate(prop, lit > 0);
                                // Don't need to look for further disjuncts
                                break;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Randomly assign values to the propositions,
        /// propagate through the initialization,
        /// and initialize the other state information accordingly.
        /// </summary>
        private void MakeRandomAssignment()
        {
            Array.Clear(falseLiterals, 0, falseLiterals.Length);
            Array.Clear(varInitialized, 0, varInitialized.Length);
            Array.Clear(alreadySatisfied, 0, alreadySatisfied.Length);
            totalUtility = 0;
            improvablePropositions.Clear();
            var vars = Problem.SATVariables;

            //
            // Set indices to a randomly permuted series of 1 .. Propositions.Length - 1
            // This gives us a random order in which to initialize the variables
            //
            var indices = new ushort[Propositions.Length - 1];
            for (var i = 0; i < indices.Length; i++)
                indices[i] = (ushort)(i + 1);

            // Fisher-Yates shuffle algorithm (https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle)
            var lastIndex = indices.Length-1;
            for (var i = 0; i < indices.Length-2; i++)
            {
                var swapWith = Random.InRange(i, lastIndex);
                var temp = indices[i];
                indices[i] = indices[swapWith];
                indices[swapWith] = temp;
            }
            
            foreach (var i in indices)
            {
                if (!varInitialized[i])
                {
                    var satVar = vars[i];
                    var truth = satVar.IsPredetermined ? satVar.PredeterminedValue : satVar.RandomInitialState;
                    // Skip the propagation if user specified
                    if (Problem.SkipPropagation)
                    {
                        Propositions[i] = truth;
                        UpdateUtility(i);
                    }
                    else
                    {
                        Propagate(i, truth);
                    }
                }
            }


            UnsatisfiedClauses.Clear();

            // Initialize trueDisjunctCount[] and unsatisfiedClauses
            for (ushort i = 0; i < TrueDisjunctCount.Length; i++)
            {
                var c = Problem.Constraints[i];
                var satisfiedDisjuncts = c.CountDisjuncts(Solution);
                TrueDisjunctCount[i] = satisfiedDisjuncts;
                if (!c.IsSatisfied(satisfiedDisjuncts) && c.IsEnabled(Solution))
                    UnsatisfiedClauses.Add(i);
            }

            //CheckUtility();
        }
        #endregion

        #region Dynamic sets
        /// <summary>
        /// Fast implementation of a dynamic set of ushorts less than some maximum value.
        /// uses O(max) space.
        /// </summary>
        public struct DynamicUShortSet
        {
            /// <summary>
            /// Make an initially empty set of ushorts no larger than max-1.
            /// </summary>
            /// <param name="max">Largest allowable ushort + 1</param>
            public DynamicUShortSet(int max)
            {
                contents = new ushort[max];
                indices = new ushort[max];
                Size = 0;
                Clear();
            }

            /// <summary>
            /// Array holding the set of ushorts currently in the set
            /// </summary>
            private readonly ushort[] contents;
            /// <summary>
            /// Array for each possible ushort giving the location in contents that that short is listed
            /// or ushort.MaxValue, if it's not in the set at all.
            /// </summary>
            private readonly ushort[] indices;
            /// <summary>
            /// Number of elements in the set
            /// </summary>
            public ushort Size;

            /// <summary>
            /// Returns the i'th element of the set.
            /// </summary>
            /// <param name="i">Position in the set, from 0..Size-1</param>
            public ushort this[int i]
            {
                get => contents[i];
                set => contents[i] = value;
            }

            /// <summary>
            /// A randomly chosen element of the set.
            /// Set must currently be non-empty.
            /// </summary>
            public ushort RandomElement => contents[Random.Next() % Size];

            /// <summary>
            /// Adds an element to the set.
            /// </summary>
            public void Add(ushort elt)
            {
                var index = Size++;
                Debug.Assert(indices[elt] == ushort.MaxValue, "Adding element already in set");
                indices[elt] = index;
                contents[index] = elt;
            }

            /// <summary>
            /// Removes an element from the set
            /// </summary>
            public void Remove(ushort elt)
            {
                Debug.Assert(indices[elt] != ushort.MaxValue, "Deleting element not in set");
                // Replace elt in the contents array with the last element in the contents array
                var index = indices[elt];
                var replacement = contents[--Size];
                contents[index] = replacement;
                indices[replacement] = index;
                indices[elt] = ushort.MaxValue;
            }

            /// <summary>
            /// Remove all elements from the set
            /// </summary>
            public void Clear()
            {
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = ushort.MaxValue;
                Size = 0;
            }

            /// <summary>
            /// True if the set contains this element
            /// </summary>
            public Boolean Contains(ushort target)
            {
                return indices[target] != ushort.MaxValue;
            }

        }
        #endregion
    }
}
