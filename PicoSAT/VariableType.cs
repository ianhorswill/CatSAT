using System.Diagnostics;

namespace PicoSAT
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract class VariableType
    {
        public readonly string Name;

        protected VariableType(string name)
        {
            Name = name;
        }

        public abstract Variable Instantiate(object name, Problem p, Literal condition = null);

        public override string ToString()
        {
            return Name;
        }
    }
}
