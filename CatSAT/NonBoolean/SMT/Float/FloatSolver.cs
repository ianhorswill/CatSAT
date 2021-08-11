#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatSolver.cs" company="Ian Horswill">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static System.Collections.StructuralComparisons;

namespace CatSAT.NonBoolean.SMT.Float
{
    class FloatSolver : TheorySolver
    {
        public int MaxTries = 10;
        internal readonly List<FloatProposition> Propositions = new List<FloatProposition>();
        // All variables
        internal readonly List<FloatVariable> Variables = new List<FloatVariable>();
        // Representatives of the equivalence classes of FloatVariables
        private readonly List<FloatVariable> representatives = new List<FloatVariable>();

        /// <summary>
        /// (v, true) in this queue means v's upper bound has recently been tightened; (v, false) means v's lower bound has recently been tightened.
        /// </summary>
        private readonly Queue<Tuple<FloatVariable,bool>> propagationQueue = new Queue<Tuple<FloatVariable, bool>>();

        private static FloatVariableArrayComparer IEqualityComparer = new FloatVariableArrayComparer();

        public Dictionary<(FloatVariable, FloatVariable), FloatVariable> ProductTable = new Dictionary<(FloatVariable, FloatVariable), FloatVariable>();
        public Dictionary<(FloatVariable, FloatVariable), FloatVariable> SumTable = new Dictionary<(FloatVariable, FloatVariable), FloatVariable>();
        public Dictionary<FloatVariable, FloatVariable> SquareTable = new Dictionary<FloatVariable, FloatVariable>();
        public Dictionary<FloatVariable[], FloatVariable> ArraySumTable = new Dictionary<FloatVariable[], FloatVariable>(IEqualityComparer);
        public Dictionary<FloatVariable[], FloatVariable> AverageTable = new Dictionary<FloatVariable[], FloatVariable>(IEqualityComparer);

        /// <summary>
        /// Add clauses that follow from user-defined bounds, e.g. from transitivity.
        /// </summary>
        /// <returns></returns>
        public override string Preprocess()
        {
            void PreprecessBounds(List<ConstantBound> constraints, int sign)
            {
                foreach (var p in Propositions)
                    p.Validate();
                constraints.Sort((a, b) => sign * Comparer.Default.Compare(a.Bound, b.Bound));

                // Add forward inferences: if v < bounds[i], then v < bounds[i+1]
                for (int i = 0; i < constraints.Count - 1; i++)
                {
                    var stronger = constraints[i];
                    var weaker = constraints[i + 1];
                    Problem.AddClause(Language.Not(stronger), weaker);
                }
            }

            foreach (var v in Variables)
            {
                PreprecessBounds(v.UpperConstantBounds, 1);
                PreprecessBounds(v.LowerConstantBounds, -1);
                // Make sure there aren't any propositions dependent on the truth of functional constraint.
                // We *could* allow this, but given that functional constraint have measure zero, the only
                // way it's possible for them to be unsatisfied with non-zero probability is through
                // floating-point quantization.  That suggests that someone trying to make a proposition
                // dependent on a != b+C is probably making a mistake.
                if (v.AllFunctionalConstraints != null)
                    foreach (var f in v.AllFunctionalConstraints)
                        if (f.IsDependency)
                            throw new InvalidOperationException($"{f.Name}: Inferences dependent on the truth of functional constraint are not supported.");
            }

            return null;
        }

        public override void PropagatePredetermined(Solution s)
        {
            foreach (var p in Propositions)
                switch (p)
                {
                    case ConstantBound b:
                        if (b.Variable.PredeterminedValueInternal.HasValue)
                        {
                            var value = b.Variable.PredeterminedValueInternal.Value;
                            Problem.SetInferredValue(p, b.IsUpper?value <= b.Bound:value >= b.Bound);
                        }
                        break;

                    case VariableBound v:
                        if (v.Lhs.PredeterminedValueInternal.HasValue && v.Rhs.PredeterminedValueInternal.HasValue)
                        {
                            var lValue = v.Lhs.PredeterminedValueInternal.Value;
                            var rValue = v.Rhs.PredeterminedValueInternal.Value;
                            Problem.SetInferredValue(p, v.IsUpper? lValue <= rValue : lValue >= rValue);
                        }
                        break;
                }
        }

        /// <summary>
        /// Try to find values for the FloatVariables that are consistent with the true constraint.
        /// </summary>
        /// <param name="s">Model providing truth values for constraint</param>
        /// <returns>True if variable values found successfully</returns>
        public override bool Solve(Solution s)
        {
            ResetAll();
            FindEquivalenceClasses(s);
            FindActiveFunctionalConstraints(s);

            if (!FindSolutionBounds(s))
                // Constraint are contradictory
                return false;

            // Repeatedly attempt to sample a solution
            for (int i = 0; i < MaxTries; i++)
            {
                if (TrySample())
                    // Success
                    return true;

                // Failed; restore bounds on variables to the values bound by FindSolutionBounds()
                foreach (var v in representatives)
                    v.Bounds = v.SolutionBounds;
            }

            // We tried several samples and failed
            return false;
        }

        /// <summary>
        /// Find the equivalence classes of variables in this model
        /// </summary>
        /// <param name="s">Boolean model against which to compute equivalence classes</param>
        private void FindEquivalenceClasses(Solution s)
        {
            // Alias vars that are equated in this model
            foreach (var p in Propositions)
            {
                if (p is VariableEquation e && s[e] && e.Lhs.IsDefinedInInternal(s) && e.Rhs.IsDefinedInInternal(s))
                    FloatVariable.Equate(e.Lhs, e.Rhs);
            }

            // We can now focus on just the representatives of each equivalence class of variables
            // and ignore the rest.
            representatives.Clear();
            representatives.AddRange(Variables.Where(v => v.IsDefinedInInternal(s) && (object) v == (object) v.Representative));
        }
        
        /// <summary>
        /// Find all functional constraint that are active in solution and attach them to the
        /// representatives of their associated variables.
        /// </summary>
        private void FindActiveFunctionalConstraints(Solution solution)
        {
            foreach (var v in Variables)
                if (v.AllFunctionalConstraints != null)
                {
                    var r = v.Representative;
                    foreach (var f in v.AllFunctionalConstraints) 
                        if (f.IsDefinedIn(solution))
                            r.AddActiveFunctionalConstraint(f);
                }
        }

        /// <summary>
        /// Find the tightest bounds we can for each representative given the model.
        /// Save these bounds in SolutionBounds field of each variable
        /// </summary>
        /// <param name="s">Model against which to compute bounds</param>
        /// <returns>False if constraint in this model are contradictory</returns>
        private bool FindSolutionBounds(Solution s)
        {
            // Apply all constant bounds that apply in this model
            foreach (var v in Variables)
            {
                if (!v.IsDefinedInInternal(s))
                    continue;

                var r = v.Representative;

                if (!ReferenceEquals(r, v) && (!r.BoundAbove(v.Bounds.Upper) || !r.BoundBelow(v.Bounds.Lower)))
                    return false;

                foreach (var b in v.UpperConstantBounds)
                    if (s[b])
                    {
                        if (!r.BoundAbove(b.Bound))
                            return false;
                    }
                    else if (b.IsDependency && !r.BoundBelow(b.Bound))
                        return false;

                foreach (var b in v.LowerConstantBounds)
                    if (s[b])
                    {
                        if (!r.BoundBelow(b.Bound))
                            return false;
                    }
                    else if (b.IsDependency && !r.BoundAbove(b.Bound))
                        return false;
            }


            // Apply all variable bounds that apply in this model
            foreach (var p in Propositions)
            {
                if (p is VariableBound b && b.Lhs.IsDefinedInInternal(s) && b.Rhs.IsDefinedInInternal(s))
                {
                    var l = b.Lhs.Representative;
                    var r = b.Rhs.Representative;

                    if (s[b] || b.IsDependency)
                    {
                        if (b.IsUpper ^ s[b])
                        {
                            r.AddUpperBound(l);
                            l.AddLowerBound(r);
                        }
                        else
                        {
                            l.AddUpperBound(r);
                            r.AddLowerBound(l);
                        }
                    }
                }
            }

            // Propagate to fixpoint
            propagationQueue.Clear();

            foreach (var v in representatives)
            {
                propagationQueue.Enqueue(new Tuple<FloatVariable, bool>(v, true));
                propagationQueue.Enqueue(new Tuple<FloatVariable, bool>(v, false));
            }

            if (!PropagateUpdates()) 
                return false;

            // Save bounds
            foreach (var v in representatives)
            {
                v.SolutionBounds = v.Bounds;
            }

            return true;
        }

        /// <summary>
        /// Reset bounds on all variables to their initial values
        /// </summary>
        private void ResetAll()
        {
            foreach (var v in Variables) v.ResetSolverState();
        }

        private bool TrySample()
        {
            Random.Shuffle(representatives);
            foreach (var v in representatives)
            {
                if (v.FloatDomain.Quantization == 0)
                {

                    v.PickValue(Random.Float(v.Bounds.Lower, v.Bounds.Upper), propagationQueue);
                }

                else
                {
                    int possibilities = (int)((v.Bounds.Upper - v.Bounds.Lower) / v.FloatDomain.Quantization);

                    // ReSharper disable once RedundantNameQualifier
                    int randStep = CatSAT.Random.InRange(0, possibilities);

                    float rand = randStep * v.FloatDomain.Quantization + v.Bounds.Lower;

                    v.PickValue(rand, propagationQueue);
                }

                if (!PropagateUpdates()) 
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Iterate until fixedpoint.
        /// </summary>
        /// <returns>True if successful, false if contradiction found</returns>
        private bool PropagateUpdates()
        {
            while (propagationQueue.Count > 0)
            {
                var work = propagationQueue.Dequeue();
                var v = work.Item1;
                var isUpper = work.Item2;

                // Propagate functional dependencies
                if (v.ActiveFunctionalConstraints != null)
                    foreach (var c in v.ActiveFunctionalConstraints)
                        if (!c.Propagate(v, isUpper, propagationQueue))
                            return false;

                // Propagate variable bounds
                if (isUpper)
                {
                    // V's upper bound decreased
                    if (v.LowerVariableBounds == null)
                        continue;

                    foreach (var dependent in v.LowerVariableBounds)
                        // So dependent's upper bound may have decreased
                        if (!dependent.BoundAbove(v.Bounds.Upper, propagationQueue))
                            return false;
                }
                else
                {
                    // V's lower bound increased
                    if (v.UpperVariableBounds == null)
                        continue;

                    foreach (var dependent in v.UpperVariableBounds)
                        // So dependent's lower bound may have decreased
                        if (!dependent.BoundBelow(v.Bounds.Lower, propagationQueue))
                            return false;
                }
            }
            return true;
        }
    }
}
