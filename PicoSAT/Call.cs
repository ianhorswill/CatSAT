using System.Diagnostics;
using System.Text;

namespace PicoSAT
{
    /// <summary>
    /// Represents a call to a predicate or function with specific arguments.
    /// This gets used as the name of the Proposition that the predicate returns when you call it.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class Call
    {
        public readonly string Name;
        public readonly object[] Args;

        public Call(string name, params object[] args)
        {
            Name = name;
            Args = args;
        }

        public override int GetHashCode()
        {
            var hash = Name.GetHashCode();
            foreach (var a in Args)
                hash ^= a.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is Call c)
            {
                if (Name != c.Name)
                    return false;
                if (Args.Length != c.Args.Length)
                    return false;
                for (int i = 0; i < Args.Length; i++)
                    if (!Args[i].Equals(c.Args[i]))
                        return false;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Name);
            b.Append('(');
            var firstOne = true;
            foreach (var a in Args)
            {
                if (firstOne)
                    firstOne = false;
                else
                    b.Append(", ");
                b.Append(a);
            }

            b.Append(')');
            return b.ToString();
        }

        private string DebuggerDisplay => ToString();
    }
}
