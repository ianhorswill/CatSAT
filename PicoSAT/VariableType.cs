using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PicoSAT
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract class VariableType
    {
        public readonly string Name;

        public static readonly Dictionary<string, VariableType> Types = new Dictionary<string, VariableType>();

        protected VariableType(string name)
        {
            Name = name;
            Types[Name] = this;
        }

        public static VariableType TypeNamed(string n)
        {
            return Types[n];
        }

        public static bool TypeExists(string n)
        {
            return Types.ContainsKey(n);
        }

        public abstract Variable Instantiate(object name, Problem p, Literal condition = null);

        public override string ToString()
        {
            return Name;
        }
    }
}
