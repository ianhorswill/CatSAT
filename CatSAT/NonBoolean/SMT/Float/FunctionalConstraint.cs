#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionalConstraint.cs" company="Ian Horswill">
// Copyright (C) 2018, 2019 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
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
        protected FloatVariable Result;

        public override void Initialize(Problem p)
        {
            base.Initialize(p);
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

        /// <summary>
        /// True when the constraint is valid in the solution.
        /// This requires that all its variables be defined.
        /// </summary>
        /// <param name="s">Solution to check</param>
        /// <returns>True if the constraint is valid in the solution and all its variables are defined.</returns>
        public abstract bool IsDefinedIn(Solution s);

        /// <summary>
        /// Returns a condition (i.e. Literal or null) that is defined when both argument
        /// conditions are defined.
        /// </summary>
        internal static Literal CombineConditions(Literal a, Literal b)
        {
            if (ReferenceEquals(a, null))
                return b;
            if (ReferenceEquals(b, null))
                return a;
            return Problem.Current.Conjunction(a, b);
        }
    }
}
