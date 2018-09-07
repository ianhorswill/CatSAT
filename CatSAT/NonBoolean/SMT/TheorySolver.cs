#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TheorySolver.cs" company="Ian Horswill">
// Copyright (C) 2018 Ian Horswill
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
namespace CatSAT
{
    /// <summary>
    /// A special purpose solver used to find values of TheoryVariables after the main SAT solver has found a partial model.
    /// </summary>
    public abstract class TheorySolver
    {
        /// <summary>
        /// Problem on which this TheorySolver is to work.
        /// </summary>
        protected Problem Problem;

        /// <summary>
        /// Makes a new theory solver
        /// This is needed just because of issues with C# generics.
        /// </summary>
        /// <param name="p">Problem this solver is assigned to</param>
        /// <typeparam name="T">Type of TheorySolver to make.</typeparam>
        /// <returns></returns>
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
