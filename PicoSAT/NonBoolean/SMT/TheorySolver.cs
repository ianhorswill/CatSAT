using System;
using System.Collections.Generic;

namespace PicoSAT
{
    public abstract class TheorySolver
    {
        protected Problem Problem;

        public static T MakeTheorySolver<T>(Problem p) where T : TheorySolver, new()
        {
            return new T() { Problem = p };
        } 

        /// <summary>
        /// Add any necessary clauses before the start of the solving process
        /// </summary>
        /// <returns>Error message (string), if an inconsistency is detected, otherwise null</returns>
        public virtual string Preprocess()
        {
            return null;
        }

        /// <summary>
        /// Find values for the solver variables
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool Solve(Solution s);
    }
}
