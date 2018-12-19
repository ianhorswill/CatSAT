using System;
using System.Collections.Generic;

namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// Base class for constraints such as SumConstraint.
    /// Represents the constraint that Result = f(args) for some f and args
    /// </summary>
    abstract class FunctionalConstraint : FloatProposition
    {
        /// <summary>
        /// The 
        /// </summary>
        protected readonly FloatVariable Result;

        protected FunctionalConstraint(FloatVariable result)
        {
            Result = result;
            Result.AddFunctionalConstraint(this);
        }

        /// <summary>
        /// Called when the bounds on a variable involved in this constraint have changed
        /// </summary>
        /// <param name="changed">Variable whose bound has changed</param>
        /// <param name="isUpper">True if the variable's upper bound lowered, or false if its lower bound increased.</param>
        /// <param name="q">Propagation queue from solver</param>
        /// <returns></returns>
        public abstract bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable,bool>> q);
    }
}
