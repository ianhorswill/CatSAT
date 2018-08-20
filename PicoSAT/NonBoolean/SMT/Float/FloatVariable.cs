using System;
using System.Collections.Generic;
using PicoSAT.NonBoolean.SMT.Float;

namespace PicoSAT
{
#pragma warning disable 660,661
    public class FloatVariable : TheoryVariable<float>
#pragma warning restore 660,661
    {
        public FloatVariable(object name, FloatDomain d, Literal condition, Problem problem) : base(name, problem, condition)
        {
            FloatDomain = d;
            Bounds = d.Bounds;
            var floatSolver = problem.GetSolver<FloatSolver>();
            Index = floatSolver.Variables.Count;
            floatSolver.Variables.Add(this);
        }

        public FloatVariable(object name, float low, float high)
            : this(name, new FloatDomain(name.ToString(), low, high), null, Problem.Current)
        { }

        public readonly FloatDomain FloatDomain;
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

        public override float PredeterminedValue()
        {
            throw new NotImplementedException();
        }

        public override void SetPredeterminedValue(float newValue)
        {
            throw new NotImplementedException();
        }

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

        public static Proposition operator ==(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableEquation>(new Call("=", v1, v2));
        }

        public static Proposition operator !=(FloatVariable v1, FloatVariable v2)
        {
            throw new NotImplementedException();
        }

        public static Proposition operator <(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(new Call("<=", v, f));
        }

        public static Proposition operator >(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(new Call(">=", v, f));
        }

        public static Proposition operator <(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableBound>(new Call("<=", v1, v2));
        }

        public static Proposition operator >(FloatVariable v1, FloatVariable v2)
        {
            return Problem.Current.GetSpecialProposition<VariableBound>(new Call(">=", v1, v2));
        }

        public void AddUpperBound(FloatVariable bound)
        {
            if (UpperVariableBounds == null)
                UpperVariableBounds = new List<FloatVariable>();
            UpperVariableBounds.Add(bound);
        }

        public void AddLowerBound(FloatVariable bound)
        {
            if (LowerVariableBounds == null)
                LowerVariableBounds = new List<FloatVariable>();
            LowerVariableBounds.Add(bound);
        }
    }
}
