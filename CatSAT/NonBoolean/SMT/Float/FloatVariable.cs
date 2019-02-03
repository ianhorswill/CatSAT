#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatVariable.cs" company="Ian Horswill">
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
using System.Globalization;
using CatSAT.NonBoolean.SMT.Float;

namespace CatSAT
{
#pragma warning disable 660,661
    /// <summary>
    /// An float-valued SMT variable
    /// </summary>
    [DebuggerDisplay("{Name}={DebugValueString}")]
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
        /// An float-valued SMT variable in the range low to high
        /// </summary>
        /// <param name="name">Name for the variable</param>
        /// <param name="low">Lower bound</param>
        /// <param name="high">Upper bound</param>
        /// <param name="condition">Condition under which the variable is defined</param>
        public FloatVariable(object name, float low, float high, Literal condition)
            : this(name, new FloatDomain(name.ToString(), low, high), condition, Problem.Current)
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

        /// <summary>
        /// The predetermined value for the variable, if any.
        /// </summary>
        internal float? PredeterminedValueInternal;

        internal readonly List<ConstantBound> UpperConstantBounds = new List<ConstantBound>();
        internal readonly List<ConstantBound> LowerConstantBounds = new List<ConstantBound>();

        internal List<FloatVariable> UpperVariableBounds;
        internal List<FloatVariable> LowerVariableBounds;

        /// <summary>
        /// All functional constraints applying to this variable
        /// </summary>
        internal List<FunctionalConstraint> AllFunctionalConstraints;

        /// <summary>
        /// Functional constraints that apply in this solution
        /// </summary>
        internal List<FunctionalConstraint> ActiveFunctionalConstraints;

        /// <summary>
        /// Variable to which this is aliased, if any
        /// </summary>
        private FloatVariable equivalence;

        /// <summary>
        /// Representative of this variable's equivalence class.
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

        private string DebugValueString => Bounds.IsUnique ? Bounds.Lower.ToString(CultureInfo.InvariantCulture) : Bounds.ToString();

        internal bool PickRandom(Queue<Tuple<FloatVariable,bool>> q)
        {
            var f = Random.Float(Bounds.Lower, Bounds.Upper);
            return BoundAbove(f, q) && BoundBelow(f, q);
        }

        /// <inheritdoc />
        public override object ValueInternal(Solution s)
        {
            Debug.Assert(Representative.Bounds.IsUnique);
            return Representative.Bounds.Lower;
        }

        /// <inheritdoc />
        public override float PredeterminedValue()
        {
            if (PredeterminedValueInternal == null)
                throw new InvalidOperationException($"{Name} has no predetermined value");
            return PredeterminedValueInternal.Value;
        }

        /// <inheritdoc />
        public override void SetPredeterminedValue(float newValue)
        {
            PredeterminedValueInternal = newValue;
        }
        
        /// <inheritdoc />
        public override void Reset()
        {
            PredeterminedValueInternal = null;
        }

        internal void ResetSolverState()
        {
            if (PredeterminedValueInternal == null)
                Bounds = FloatDomain.Bounds;
            else
                Bounds = new Interval(PredeterminedValueInternal.Value);
            equivalence = null;
            UpperVariableBounds?.Clear();
            LowerVariableBounds?.Clear();
            ActiveFunctionalConstraints?.Clear();
        }

        /// <summary>
        /// Asserts that variable must be no larger than bound.
        /// Updates Bounds.Upper if necessary
        /// </summary>
        /// <param name="bound">Upper bound on variable</param>
        /// <returns>True is resulting bounds are consistent</returns>
        internal bool BoundAbove(float bound)
        {
            if (!(bound < Bounds.Upper)) return true;
            Bounds.Upper = bound;
            return Bounds.IsNonEmpty;
        }

        /// <summary>
        /// Asserts that variable must be at least as large as bound.
        /// Updates Bounds.Lower if necessary
        /// </summary>
        /// <param name="bound">Lower bound on variable</param>
        /// <returns>True is resulting bounds are consistent</returns>
        internal bool BoundBelow(float bound)
        {
            if (!(bound > Bounds.Lower)) return true;
            Bounds.Lower = bound;
            return Bounds.IsNonEmpty;
        }

        /// <summary>
        /// Asserts that variable must be no larger than bound.
        /// Updates Bounds.Upper and adds variable to propagation queue, if necessary.
        /// </summary>
        /// <param name="bound">Upper bound on variable</param>
        /// <param name="q">Propagation queue</param>
        /// <returns>True is resulting bounds are consistent</returns>
        internal bool BoundAbove(float bound, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (!(bound < Bounds.Upper)) return true;
            Bounds.Upper = bound;
            EnsurePresent(q, new Tuple<FloatVariable, bool>(this, true));
            return Bounds.IsNonEmpty;
        }

        /// <summary>
        /// Assert that variable is in the specified interval.
        /// Updates Bounds and adds variable to propagation queue, if necessary. 
        /// </summary>
        /// <param name="i">Bounding interval</param>
        /// <param name="q">Propagation queue</param>
        /// <returns>True if resulting bounds are consistent</returns>
        public bool NarrowTo(Interval i, Queue<Tuple<FloatVariable, bool>> q)
        {
            return BoundAbove(i.Upper, q) && BoundBelow(i.Lower, q);
        }

//        public bool NarrowToQuotient(Interval numerator, Interval denominator, Queue<Tuple<FloatVariable, bool>> q)
//        {
//            if (denominator.IsZero)
//                // Denominator is [0,0], so quotient is the empty set unless numerator contains zero
//                return numerator.ContainsZero;

//            if (numerator.IsZero)
//            {
//                if (!denominator.ContainsZero)
//                    // Quotient is [0,0].
//                    if (!NarrowTo(new Interval(0,0), q))
//                        return false;
//                // Denominator contains zero so quotient can be any value.
//                return true;
//            }

//            if (!denominator.ContainsZero)
//                return NarrowTo(numerator * denominator.Reciprocal, q);

//            // Denominator contains zero, so there are three cases: crossing zero, [0, b], and [a, 0]

//// ReSharper disable CompareOfFloatsByEqualityOperator
//            if (denominator.Lower == 0)
//// ReSharper restore CompareOfFloatsByEqualityOperator
//            {
//                // Non-negative denominator
//                if (numerator.Upper <= 0)
//                    return NarrowTo(new Interval(float.NegativeInfinity, numerator.Upper / denominator.Upper), q);

//                if (numerator.Lower >= 0)
//                    return NarrowTo(new Interval(numerator.Lower / denominator.Upper, float.PositiveInfinity), q);
//                // Numerator crosses zero, so quotient is all the Reals, so can't narrow interval.
//                return true;
//            }

//// ReSharper disable CompareOfFloatsByEqualityOperator
//            if (denominator.Upper == 0)
//// ReSharper restore CompareOfFloatsByEqualityOperator
//            {
//                // Non-positive denominator
//                if (numerator.Upper <= 0)
//                    return NarrowTo(new Interval(numerator.Upper / denominator.Lower, float.PositiveInfinity), q);

//                if (numerator.Lower >= 0)
//                    return NarrowTo(new Interval(float.NegativeInfinity, numerator.Lower / denominator.Lower), q);
//                // Numerator crosses zero, so quotient is all the Reals, so can't narrow interval.
//                return true;
//            }

//            if (numerator.Upper < 0)
//            {
//                // Strictly negative
//                var lowerHalf = new Interval(float.NegativeInfinity, numerator.Upper / denominator.Upper);
//                var upperHalf = new Interval(numerator.Upper / denominator.Lower, float.PositiveInfinity);
//                return NarrowToUnion(lowerHalf, upperHalf, q);
//            }

//            // Denominator crosses zero
//            if (numerator.Lower > 0)
//            {
//                // Strictly positive
//                var lowerHalf = new Interval(float.NegativeInfinity, numerator.Lower / denominator.Lower);
//                var upperHalf = new Interval(numerator.Lower / denominator.Upper, float.PositiveInfinity);

//                return NarrowToUnion(lowerHalf, upperHalf, q);
//            }

//            // Numerator contains zero, so quotient is all the Reals, so can't narrow interval.
//            return true;
//        }

//        public bool NarrowToUnion(Interval a, Interval b, Queue<Tuple<FloatVariable, bool>> q)
//        {
//            return NarrowTo(Interval.UnionOfIntersections(Bounds, a, b), q);
//        }

        /// <summary>
        /// Adds the specified (variable, IsUpper) task to queue if it is not already present.
        /// </summary>
        private void EnsurePresent(Queue<Tuple<FloatVariable, bool>> q, Tuple<FloatVariable, bool> task)
        {
            if (q.Contains(task)) return;
            q.Enqueue(task);
        }

        /// <summary>
        /// Asserts that variable must be no less than bound.
        /// Updates Bounds.Lower and adds variable to propagation queue, if necessary.
        /// </summary>
        /// <param name="bound">Lower bound on variable</param>
        /// <param name="q">Propagation queue</param>
        /// <returns>True is resulting bounds are consistent</returns>
        internal bool BoundBelow(float bound, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (!(bound > Bounds.Lower)) return true;
            Bounds.Lower = bound;
            EnsurePresent(q, new Tuple<FloatVariable, bool>(this, false));
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

        /// <summary>
        /// A FloatVariable constrained to be the sum of two other FloatVariables
        /// </summary>
        public static FloatVariable operator +(FloatVariable v1, FloatVariable v2)
        {
            var sum = new FloatVariable($"{v1.Name}+{v2.Name}",
                v1.FloatDomain.Bounds.Lower+v2.FloatDomain.Bounds.Lower,
                v1.FloatDomain.Bounds.Upper+v2.FloatDomain.Bounds.Upper,
                FunctionalConstraint.CombineConditions(v1.Condition, v2.Condition));
            Problem.Current.Assert(Problem.Current.GetSpecialProposition<BinarySumConstraint>(Call.FromArgs(Problem.Current, "IsSum", sum, v1, v2)));
            return sum;
        }

        /// <summary>
        /// A FloatVariable constrained to be the product of two other FloatVariables
        /// </summary>
        public static FloatVariable operator *(FloatVariable v1, FloatVariable v2)
        {
            var range = v1.FloatDomain.Bounds * v2.FloatDomain.Bounds;
            var product = new FloatVariable($"{v1.Name}*{v2.Name}", range.Lower, range.Upper,
                FunctionalConstraint.CombineConditions(v1.Condition, v2.Condition));
            Problem.Current.Assert(Problem.Current.GetSpecialProposition<ProductConstraint>(Call.FromArgs(Problem.Current, "IsProduct", product, v1, v2)));
            return product;
        }

        /// <summary>
        /// A FloatVariable constrained to be the product of another FloatVariable and a constant
        /// </summary>
        public static FloatVariable operator *(float c, FloatVariable v)
        {
            return MonotoneFunctionConstraint.MonotoneFunction($"*{c}", x => x * c, y => y / c, c > 0, v);
        }
        
        /// <summary>
        /// A FloatVariable constrained to be the product of another FloatVariable and a constant
        /// </summary>
        public static FloatVariable operator *(FloatVariable v, float c)
        {
            return c * v;
        }

        public static FloatVariable Sum(params FloatVariable[] vars)
        {
            var domainBounds = Interval.Zero;
            foreach (var a in vars)
                domainBounds += a.FloatDomain.Bounds;
            foreach (var a in vars)
                if (!ReferenceEquals(a.Condition,null))
                    throw new ArgumentException("Sum does not support conditioned variables", a.Name.ToString());
            var sum = new FloatVariable("sum", domainBounds.Lower, domainBounds.Upper, null);
            Problem.Current.Assert(Problem.Current.GetSpecialProposition<GeneralSumConstraint>(Call.FromArgs(Problem.Current, "IsSum", sum, 1.0f, vars)));
            return sum;
        }

        public static FloatVariable Average(params FloatVariable[] vars)
        {
            var domainBounds = Interval.Zero;
            foreach (var a in vars)
                domainBounds += a.FloatDomain.Bounds;
            foreach (var a in vars)
                if (!ReferenceEquals(a.Condition,null))
                    throw new ArgumentException("Average does not support conditioned variables", a.Name.ToString());
            var avg = new FloatVariable("average",
                domainBounds.Lower*1f/vars.Length,
                domainBounds.Upper*1f/vars.Length,
                null);
            Problem.Current.Assert(Problem.Current.GetSpecialProposition<GeneralSumConstraint>(Call.FromArgs(Problem.Current, "IsAverage", avg, 1f/vars.Length, vars)));
            return avg;
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

        internal void AddFunctionalConstraint(FunctionalConstraint c)
        {
            if (AllFunctionalConstraints == null)
            {
                AllFunctionalConstraints = new List<FunctionalConstraint>();
            }
            AllFunctionalConstraints.Add(c);
        }

        internal void AddActiveFunctionalConstraint(FunctionalConstraint f)
        {
            if (ActiveFunctionalConstraints == null)
                ActiveFunctionalConstraints = new List<FunctionalConstraint>();
            if (!ActiveFunctionalConstraints.Contains(f))
                ActiveFunctionalConstraints.Add(f);
        }
    }
}
