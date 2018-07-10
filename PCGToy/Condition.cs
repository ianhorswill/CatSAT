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
                var p = Variable.SolverVariable == Value;
                return Positive ? p : Not(p);
            }
        }
    }
}
