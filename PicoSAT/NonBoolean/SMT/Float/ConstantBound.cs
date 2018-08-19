using System.Diagnostics;

namespace PicoSAT.NonBoolean.SMT.Float
{
    [DebuggerDisplay("{Variable.Name} {Operator} {Bound}")]
    class ConstantBound : FloatProposition
    {
        public override void Initialize(Problem p)
        {
            base.Initialize(p);
            var c = (Call)Name;
            Variable = (FloatVariable)c.Args[0];
            Bound = (float)c.Args[1];
            (IsUpper?Variable.UpperBounds:Variable.LowerBounds).Add(this);
        }

        public FloatVariable Variable;
        public float Bound;
        public bool IsUpper => Operator == "<=";

        private string Operator => ((Call) Name).Name;
    }
}
