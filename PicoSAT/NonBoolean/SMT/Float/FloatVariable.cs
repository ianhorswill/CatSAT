using System;
using PicoSAT.NonBoolean.SMT.Float;

namespace PicoSAT
{
    public class FloatVariable : TheoryVariable<float>
    {
        public FloatVariable(object name, FloatDomain d, Literal condition, Problem problem) : base(name, problem, condition)
        {
            FloatDomain = d;
            var floatSolver = problem.GetSolver<FloatSolver>();
            Index = floatSolver.Variables.Count;
            floatSolver.Variables.Add(this);
        }

        public readonly FloatDomain FloatDomain;
        public override Domain<float> Domain => FloatDomain;

        /// <summary>
        /// Position in the FloatSolver's list of variables.
        /// </summary>
        internal readonly int Index;

        public override float Value(Solution s)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
