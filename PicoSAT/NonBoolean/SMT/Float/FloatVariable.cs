using System;
using System.Collections.Generic;
using PicoSAT.NonBoolean.SMT.Float;

namespace PicoSAT
{
    public class FloatVariable : TheoryVariable<float>
    {
        public FloatVariable(object name, FloatDomain d, Literal condition, Problem problem) : base(name, problem, condition)
        {
            FloatDomain = d;
            Bounds = d.Bounds;
            var floatSolver = problem.GetSolver<FloatSolver>();
            Index = floatSolver.Variables.Count;
            floatSolver.Variables.Add(this);
        }

        public readonly FloatDomain FloatDomain;
        public override Domain<float> Domain => FloatDomain;
        
        internal Interval Bounds;

        internal readonly List<ConstantBound> UpperBounds = new List<ConstantBound>();
        internal readonly List<ConstantBound> LowerBounds = new List<ConstantBound>();

        /// <summary>
        /// Position in the FloatSolver's list of variables.
        /// </summary>
        internal readonly int Index;

        public override float Value(Solution s)
        {
            if (Bounds.IsUnique)
                return Bounds.Lower;
            throw new InvalidOperationException($"Variable {Name} has not been narrowed to a unique value.");
        }

        internal void PickRandom()
        {
            var f = Random.Float(Bounds.Lower, Bounds.Upper);
            Bounds.Lower = Bounds.Upper = f;
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
        }

        public static Literal operator <(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(new Call("<=", v, f));
        }

        public static Literal operator >(FloatVariable v, float f)
        {
            return Problem.Current.GetSpecialProposition<ConstantBound>(new Call(">=", v, f));
        }
    }
}
