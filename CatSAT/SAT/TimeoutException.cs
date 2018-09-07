#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeoutException.cs" company="Ian Horswill">
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
using System;

namespace CatSAT
{
    /// <summary>
    /// Signifies the solver game up.
    /// This doesn't mean that the problem is necessarily unsatisfiable (contains a contradiction).
    /// It just means that the solver couldn't find a solution in the alloted time.
    /// </summary>
    public class TimeoutException : Exception
    {
        /// <summary>
        /// Problem for which the system couldn't find a solution.
        /// </summary>
        public readonly Problem Problem;

        internal TimeoutException(Problem problem) : base($"Could not find a solution: {problem}")
        {
            Problem = problem;
        }
    }
}
