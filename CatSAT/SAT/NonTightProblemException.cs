#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NonTightProblemException.cs" company="Ian Horswill">
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

        internal NonTightProblemException(Proposition offender) : base($"{offender} is inferrable from itself!")
        {
            Offender = offender;
        }
    }
}
