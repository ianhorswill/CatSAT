using System;

namespace CatSAT.NonBoolean
{
    /// <summary>
    /// Signifies that the program attempted to get the value of a Variable within a Solution in which is undefined.
    /// </summary>
    public class UndefinedVariableException : InvalidOperationException
    {
        /// <summary>
        /// The variable that was not defined in Solution
        /// </summary>
        public readonly Variable Variable;
        /// <summary>
        /// The solution within which Variable was undefined
        /// </summary>
        public readonly Solution Solution;

        /// <summary>
        /// Signal that the specified variable is not defined in this solution
        /// </summary>
        public UndefinedVariableException(Variable variable, Solution solution)
        {
            Variable = variable;
            Solution = solution;
        }

        /// <inheritdoc />
        public override string Message => $"The variable {Variable} is not defined in the specified solution.";
    }
}
