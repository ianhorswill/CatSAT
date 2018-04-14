using System.Collections.Generic;
using System.Diagnostics;

namespace PicoSAT
{
    /// <summary>
    /// Run-time representation of the truth assignment of a proposition in a Solution.
    /// This is kept separate from the Proposition objects themselves, partly in the hopes of
    /// improving cache performance, and partly so that the Proposition objects can be GC'ed once the
    /// clauses are computed.
    /// 
    /// Note - the actual truth value of the Variable isn't stored here, it's stored in the Solution
    /// object, since we can have multiple solutions that assign different values to the variable.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Proposition) + "}")]
    struct Variable
    {
        public Variable(Proposition proposition)
        {
            Proposition = proposition;
            PositiveClauses = new List<ushort>();
            NegativeClauses = new List<ushort>();
        }

        /// <summary>
        /// The Proposition object for which this variable holds the truth value.
        /// </summary>
        public readonly Proposition Proposition;

        public override string ToString()
        {
            return Proposition.ToString();
        }

        /// <summary>
        /// Clauses in which this variable appears unnegated.
        /// Used to know what clauses to check if we flip this variable
        /// </summary>
        public readonly List<ushort> PositiveClauses;
        /// <summary>
        /// Clauses in which this variable appears negated.
        /// Used to know what clauses to check if we flip this variable
        /// </summary>
        public readonly List<ushort> NegativeClauses;
    }
}
