using System;
using System.Collections;
using System.Collections.Generic;

namespace PicoSAT.NonBoolean.SMT.Float
{
    class FloatSolver : TheorySolver
    {
        internal List<FloatProposition> Propositions = new List<FloatProposition>();
        internal List<FloatVariable> Variables = new List<FloatVariable>();

        public override string Preprocess()
        {
            void PreprecessBounds(List<ConstantBound> constraints, int sign)
            {
                constraints.Sort((a, b) => sign*Comparer.Default.Compare(a.Bound, b.Bound));

                // Add forward inferences: if v < bounds[i], then v < bounds[i+1]
                for (int i = 0; i < constraints.Count - 1; i++)
                {
                    var stronger = constraints[i];
                    var weaker = constraints[i + 1];
                    Problem.AddClause(Language.Not(stronger), weaker);
                }
            }

            foreach (var v in Variables)
            {
                PreprecessBounds(v.UpperBounds, 1);
                PreprecessBounds(v.LowerBounds, -1);
            }

            return null;
        }

        public override bool Solve(Solution s)
        {
            foreach (var v in Variables)
                v.Bounds = v.FloatDomain.Bounds;

            foreach (var v in Variables)
            {
                foreach (var b in v.UpperBounds)
                    if (s[b])
                    {
                        v.Bounds.Upper = b.Bound;
                        break;
                    }

                foreach (var b in v.LowerBounds)
                    if (s[b])
                    {
                        v.Bounds.Lower = b.Bound;
                        break;
                    }
            }

            foreach (var v in Variables)
                v.PickRandom();

            return true;
        }
    }
}
