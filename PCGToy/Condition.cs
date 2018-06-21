using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicoSAT;
using static PicoSAT.Language;

namespace PCGToy
{
    public class Condition
    {
        public bool Positive;
        public Variable Variable;
        public object Value;

        public Condition(bool positive, Variable variable, object value)
        {
            Positive = positive;
            Variable = variable;
            Value = value;
        }

        public Literal Literal
        {
            get
            {
                var p = Variable.predicate(Value);
                return Positive ? p : Not(p);
            }
        }
    }
}
