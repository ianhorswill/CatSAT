using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    class MenuProposition<T> : TheoryProposition
    {
        public override void Initialize(Problem p)
        {
            p.GetSolver<MenuSolver<T>>().Propositions.Add(this);
        }
    }
}
