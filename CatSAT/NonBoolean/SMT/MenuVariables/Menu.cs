using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    public class Menu<T> : Domain<T>
    {
        public Menu(string name, T[] selections) : base(name)
        {
            Selections = selections;
        }

        public readonly T[] Selections;

        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            return new MenuVariable<T>(name, this, p, condition);
        }
    }
}
