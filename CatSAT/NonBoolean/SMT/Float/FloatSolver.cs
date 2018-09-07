#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatSolver.cs" company="Ian Horswill">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CatSAT.NonBoolean.SMT.Float
{
    class FloatSolver : TheorySolver
    {
        public int MaxTries = 10;
        internal readonly List<FloatProposition> Propositions = new List<FloatProposition>();
        // All variables
        internal readonly List<FloatVariable> Variables = new List<FloatVariable>();
        // All variables not aliased to other variables by equality assertions.
        private readonly List<FloatVariable> activeVariables = new List<FloatVariable>();

        private readonly Queue<Tuple<FloatVariable,bool>> propagationQueue = new Queue<Tuple<FloatVariable, bool>>();

        public override string Preprocess()
        {
            void PreprecessBounds(List<ConstantBound> constraints, int sign)
            {
                constraints.Sort((a, b) => sign*Comparer.Default.Compare(a.Bound, b.Bound));

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
            }

            return null;
        }

        public override bool Solve(Solution s)
        {
            foreach (var v in Variables) v.Reset();

            foreach (var p in Propositions)
            {
                if (p is VariableEquation e && s[e])
                    FloatVariable.Equate(e.Lhs, e.Rhs);
            }

            foreach (var v in Variables)
            {
                var r = v.Representative;

                if (!ReferenceEquals(r, v) && (!r.BoundAbove(v.Bounds.Upper) || !r.BoundBelow(v.Bounds.Lower)))
                    return false;

                foreach (var b in v.UpperConstantBounds)
                    if (s[b])
                    {
                        if (r.BoundAbove(b.Bound)) break;
                        return false;
                    }

                foreach (var b in v.LowerConstantBounds)
                    if (s[b])
                    {
                        if (r.BoundBelow(b.Bound)) break;
                        return false;
                    }
            }

            activeVariables.Clear();
            activeVariables.AddRange(Variables.Where(v=> (object)v == (object)v.Representative));

            foreach (var p in Propositions)
            {
                if (p is VariableBound b && s[b])
                {
                    var l = b.Lhs.Representative;
                    var r = b.Rhs.Representative;
                    if (b.IsUpper)
                    {
                        l.AddUpperBound(r);
                        r.AddLowerBound(l);
                    }
                    else
                    {
                        r.AddUpperBound(l);
                        l.AddLowerBound(r);
                    }
                }
            }

            propagationQueue.Clear();

            foreach (var v in activeVariables)
            {
                propagationQueue.Enqueue(new Tuple<FloatVariable, bool>(v, true));
                propagationQueue.Enqueue(new Tuple<FloatVariable, bool>(v, false));
            }

            if (!PropagateUpdates()) return false;

            // Save bounds
            foreach (var v in activeVariables)
                v.SolutionBounds = v.Bounds;

            for (int i = 0; i < MaxTries; i++)
            {
                if (TrySample()) return true;

                // Restore bounds
                foreach (var v in activeVariables)
                    v.Bounds = v.SolutionBounds;
            }

            // We tried several samples and failed
            return false;
        }

        private bool TrySample()
        {
            foreach (var v in activeVariables)
            {
                v.PickRandom(propagationQueue);
                if (!PropagateUpdates()) return false;
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

                if (isUpper)
                {
                    // V's upper bound decreased
                    if (v.LowerVariableBounds == null)
                        continue;

                    foreach (var dependent in v.LowerVariableBounds)
                    {
                        // So dependent's upper bound may have decreased
                        if (!dependent.BoundAbove(v.Bounds.Upper, propagationQueue))
                            return false;
                    }
                }
                else
                {
                    // V's lower bound increased
                    if (v.UpperVariableBounds == null)
                        continue;

                    foreach (var dependent in v.UpperVariableBounds)
                    {
                        // So dependent's lower bound may have decreased
                        if (!dependent.BoundBelow(v.Bounds.Lower, propagationQueue))
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
