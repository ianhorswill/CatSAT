#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatDomain.cs" company="Ian Horswill">
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
namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// A domain of single-precision floating-point numbers.
    /// Domain must be a closed interval.
    /// </summary>
    public class FloatDomain : Domain<float>
    {
        /// <summary>
        /// The upper and lower bounds of the domain
        /// </summary>
        public readonly Interval Bounds;
        /// <summary>
        /// A floating-point domain, specified by upper- and lower-bounds
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="lowerBound">Lower bound of the variable</param>
        /// <param name="upperBound">Upper bound of the variable</param>
        public FloatDomain(string name, float lowerBound, float upperBound) : base(name)
        {
            Bounds = new Interval(lowerBound, upperBound);
        }

        /// <inheritdoc />
        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            return new FloatVariable(name, this, condition, p);
        }
    }
}
