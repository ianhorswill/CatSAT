using System;

namespace PicoSAT
{
    /// <summary>
    /// Signifies that a problem contains a circular definition.
    /// That is, it contains a proposition that can be inferred from itself.
    /// </summary>
    public class NonTightProblemException : Exception
    {
        /// <summary>
        /// Proposition that can be inferred from itself.
        /// This is generally not the only such proposition, it's just the first one the solver found.
        /// </summary>
        public readonly Proposition Offender;

        public NonTightProblemException(Proposition offender) : base($"{offender} is inferrable from itself!")
        {
            Offender = offender;
        }
    }
}
