using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT.NonBoolean.SMT.Float
{
    internal class VariableEquation : FloatProposition
    {
        public override void Initialize(Problem p)
        {
            base.Initialize(p);
            var c = (Call)Name;
            Lhs = (FloatVariable)c.Args[0];
            Rhs = (FloatVariable)c.Args[1];
        }

        public FloatVariable Lhs, Rhs;
    }
}
