namespace PicoSAT.NonBoolean.SMT.Float
{
    class VariableBound : FloatProposition
    {
        public override void Initialize(Problem p)
        {
            base.Initialize(p);
            var c = (Call)Name;
            Lhs = (FloatVariable)c.Args[0];
            Rhs = (FloatVariable)c.Args[1];
        }

        public FloatVariable Lhs, Rhs;

        public bool IsUpper => Operator == "<=";

        private string Operator => ((Call) Name).Name;
    }
}
