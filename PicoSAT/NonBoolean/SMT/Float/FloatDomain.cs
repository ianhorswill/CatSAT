using System;

namespace PicoSAT.NonBoolean.SMT.Float
{
    public class FloatDomain : Domain<float>
    {
        public readonly Interval Bounds;
        public FloatDomain(string name, float lowerBound, float upperBound) : base(name)
        {
            Bounds = new Interval(lowerBound, upperBound);
        }

        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            return new FloatVariable(name, this, condition, p);
        }
    }
}
