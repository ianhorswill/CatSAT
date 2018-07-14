using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT
{
    public static class VariableProblemExtensions
    {
        public static Variable Instantiate(this Problem p, object name, VariableType t, Literal condition = null)
        {
            return t.Instantiate(name, p, condition);
        }

        public static void AllDifferent<T>(this Problem p, IEnumerable<FDVariable<T>> vars)
        {
            var d = (FDomain<T>)vars.First().Domain;
            foreach (var v in vars)
                if (v.Domain != d)
                    throw new ArgumentException($"Variables in AllDifferent() call must have identical domains");
            foreach (var value in d.Values)
                p.AtMost(1, vars.Select(var => var == value));
        }
    }
}
