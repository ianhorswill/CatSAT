#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatVariable.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using CatSAT.NonBoolean.SMT.Float;

namespace CatSAT
{
#pragma warning disable 660,661
    /// <summary>
    /// An float-valued SMT variable
    /// </summary>
    public class FloatVariable : TheoryVariable<float>
#pragma warning restore 660,661
    {
        /// <inheritdoc />
        public FloatVariable(object name, FloatDomain d, Literal condition, Problem problem) : base(name, problem, condition)
        {
            FloatDomain = d;
            Bounds = d.Bounds;
            var floatSolver = problem.GetSolver<FloatSolver>();
            Index = floatSolver.Variables.Count;
            floatSolver.Variables.Add(this);
        }

        /// <summary>
        /// An float-valued SMT variable in the range low to high
        /// </summary>
        /// <param name="name">Name for the variable</param>
        /// <param name="low">Lower bound</param>
        /// <param name="high">Upper bound</param>
        public FloatVariable(object name, float low, float high)
            : this(name, new FloatDomain(name.ToString(), low, high), null, Problem.Current)
        { }

        /// <summary>
        /// Domain for the variable
        /// </summary>
        public readonly FloatDomain FloatDomain;

        /// <inheritdoc />
        public override Domain<float> Domain => FloatDomain;

        /// <summary>
        /// Saved bounds for current SMT Solution, after initial constraint processing, but before sampling
        /// </summary>
        internal Interval SolutionBounds;

        /// <summary>
        /// Current bounds to which the variable has been narrowed
        /// </summary>
        internal Interval Bounds;

        internal readonly List<ConstantBound> UpperConstantBounds = new List<ConstantBound>();
        internal readonly List<ConstantBound> LowerConstantBounds = new List<ConstantBound>();

        internal List<FloatVariable> UpperVariableBounds;
        internal List<FloatVariable> LowerVariableBounds;

        /// <summary>
        /// Variable to which this is aliased, if any
        /// </summary>
        private FloatVariable equivalence;

        /// <summary>
        /// Representative of this variable's equivalance class.
        /// This will be the variable itself, unless it has been aliased to something else by
        /// == proposition
        /// </summary>
        internal FloatVariable Representative
        {
            get
            {
                var v = this;
                while ((object)v.equivalence != null) v = v.equivalence;
                return v;
            }
        }

        internal static void Equate(FloatVariable v1, FloatVariable v2)
        {
            v1.equivalence = v2.Representative;
        }

        /// <summary>
        /// Position in the FloatSolver's list of variables.
        /// </summary>
        internal readonly int Index;

        /// <inheritdoc />
        public override float Value(Solution s)
        {
            var r = Representative;
            if (r.Bounds.IsUnique)
                return r.Bounds.Lower;
            throw new InvalidOperationException($"Variable {Name} has not been narrowed to a unique value.");
        }

        internal bool PickRandom(Queue<Tuple<FloatVariable,bool>> q)
        {
            var f = Random.Float(Bounds.Lower, Bounds.Upper);
            if (BoundAbove(f, q))
                if (BoundBelow(f, q))
                    return true;
            return false;
        }

        /// <inheritdoc />
        public override float PredeterminedValue()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetPredeterminedValue(float newValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            Bounds = FloatDomain.Bounds;
            equivalence = null;
            UpperVariableBounds?.Clear();
            LowerVariableBounds?.Clear();
        }

        internal bool BoundAbove(float bound)
        {
            if (!(bound < Bounds.Upper)) return true;
            Bounds.Upper = bound;
            return Bounds.IsNonEmpty;
        }

        internal bool BoundBelow(float bound)
        {
            if (!(bound > Bounds.Lower)) return true;
            Bounds.Lower = bound;
            return Bounds.IsNonEmpty;
        }

        internal bool BoundAbove(float bound, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (!(bound < Bounds.Upper)) return true;
            Bounds.Upper = bound;
            EnsurePresent(q, new Tuple<FloatVariable, bool>(this, true));
            return Bounds.IsNonEmpty;
        }

        private void EnsurePresent(Queue<Tuple<FloatVariable, bool>> q, Tuple<FloatVariable, bool> task)
        {
            if (q.Contains(task)) return;
            q.Enqueue(task);
        }

        internal bool BoundBelow(float bound, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (!(bound > Bounds.Lower)) return true;
            Bounds.Lower = bound;
            return Bounds.IsNonEmpty;
        }

        /// <summary>
        /// A proposition representing that the two variables are equal
        /// </summary>
        /// <param name="v1">First variable</param>
        /// <param name="v2">Second variable</param>
        public static Proposition operator ==(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableEquation>(Call.FromArgs(Problem.Current, "=", v1, v2));
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Proposition operator !=(FloatVariable v1, FloatVariable v2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A proposition representing that the variable is less than OR EQUAL to a constant
        /// </summary>
        /// <param name="v">Variable bounded by constant</param>
        /// <param name="f">Upper bound on the variable</param>
        /// <returns></returns>
        public static Proposition operator <(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(Call.FromArgs(Problem.Current, "<=", v, f));
        }

        /// <summary>
        /// A proposition representing that the variable is greater than OR EQUAL to a constant
        /// </summary>
        /// <param name="v">Variable bounded by constant</param>
        /// <param name="f">lower bound on the variable</param>
        /// <returns></returns>
        public static Proposition operator >(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(Call.FromArgs(Problem.Current, ">=", v, f));
        }

        /// <summary>
        /// A proposition representing that the one variable is less than OR EQUAL to another
        /// </summary>
        /// <param name="v1">Smaller variable</param>
        /// <param name="v2">Larger variable</param>
        /// <returns></returns>
        public static Proposition operator <(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableBound>(Call.FromArgs(Problem.Current, "<=", v1, v2));
        }

        /// <summary>
        /// A proposition representing that the one variable is less than OR EQUAL to another
        /// </summary>
        /// <param name="v1">Larger variable</param>
        /// <param name="v2">Smaller variable</param>
        /// <returns></returns>
        public static Proposition operator >(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableBound>(Call.FromArgs(Problem.Current, ">=", v1, v2));
        }

        internal void AddUpperBound(FloatVariable bound)
        {
            if (UpperVariableBounds == null)
                UpperVariableBounds = new List<FloatVariable>();
            UpperVariableBounds.Add(bound);
        }

        internal void AddLowerBound(FloatVariable bound)
        {
            if (LowerVariableBounds == null)
                LowerVariableBounds = new List<FloatVariable>();
            LowerVariableBounds.Add(bound);
        }
    }
}
