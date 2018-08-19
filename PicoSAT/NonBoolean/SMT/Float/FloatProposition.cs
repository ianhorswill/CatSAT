namespace PicoSAT.NonBoolean.SMT.Float
{
    class FloatProposition : TheoryProposition
    {
        public override void Initialize(Problem p)
        {
            p.GetSolver<FloatSolver>().Propositions.Add(this);
        }
    }
}
