using System.Diagnostics;

namespace PicoSAT
{
    /// <summary>
    /// Base class for typed, non-Boolean variables, either for NBSAT or for SMT
    /// </summary>
    /// <typeparam name="T">Type of the variable's value</typeparam>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract class DomainVariable<T> : Variable
    {
        protected DomainVariable(object name, Problem problem, Literal condition) : base(name, problem, condition)
        {
        }

        /// <summary>
        /// Domain specifying the possible values of the variable
        /// </summary>
        public abstract Domain<T> Domain { get; }
        
        /// <summary>
        /// Returns the value assigned to the variable in the solution
        /// </summary>
        /// <param name="s">Solution from which to get the variable's value</param>
        /// <returns>Value of the variable</returns>
        public abstract T Value(Solution s);

        public override object UntypedValue(Solution s)
        {
            return Value(s);
        }

        /// <summary>
        /// Returns the value assigned to the variable in the problem, if any
        /// </summary>
        /// <returns>Value of the variable</returns>
        public abstract T PredeterminedValue();

        /// <summary>
        /// Fixes the value assigned to the variable in the problem.
        /// </summary>
        /// <param name="newValue"></param>
        public abstract void SetPredeterminedValue(T newValue);

        /// <summary>
        /// Removes any predetermined value for the variable
        /// </summary>
        public abstract void Reset();
    }
}
