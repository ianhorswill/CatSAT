using System;

namespace PicoSAT
{
    /// <summary>
    /// Signifies the Problem has no solution.
    /// </summary>
    public class UnsatisfiableException : Exception
    {
        public readonly Problem Problem;

        public UnsatisfiableException(Problem problem) : base($"Program is unsatisfiable: {problem}")
        {
            Problem = problem;
        }
    }
}
