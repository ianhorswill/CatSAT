namespace PicoSAT.NonBoolean.SMT.Float
{
    class FloatProposition : TheoryProposition
    {
        public FloatProposition(Problem p)
        {
            p.GetSolver<FloatSolver>().Propositions.Add(this);
        }
    }
}
