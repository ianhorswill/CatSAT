using System;

namespace PicoSAT.NonBoolean.SMT.Float
{
    public class FloatDomain : Domain<float>
    {
        public FloatDomain(string name) : base(name)
        {
        }

        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            throw new NotImplementedException();
        }
    }
}
