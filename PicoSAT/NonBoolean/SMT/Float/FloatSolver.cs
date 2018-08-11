using System;
using System.Collections.Generic;

namespace PicoSAT.NonBoolean.SMT.Float
{
    class FloatSolver : TheorySolver
    {
        public FloatSolver(Problem p) : base(p)
        { }

        internal List<FloatProposition> Propositions = new List<FloatProposition>();
        internal List<FloatVariable> Variables = new List<FloatVariable>();

        static FloatSolver()
        {
            TheorySolver.constructors[typeof(FloatSolver)] = p => new FloatSolver(p);
        }

        public override bool Solve(Solution s)
        {
            throw new NotImplementedException();
        }
    }
}
