#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Interval.cs" company="Ian Horswill">
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
    /// A closed inteval over the single-precision floating-point numbers
    /// </summary>
    public struct Interval
    {
        /// <summary>
        /// An interval over the floats
        /// </summary>
        /// <param name="lowerBound">Lower bound</param>
        /// <param name="upperBound">Upper bound</param>
        public Interval(float lowerBound, float upperBound)
        {
            Lower = lowerBound;
            Upper = upperBound;
        }

        /// <summary>
        /// Lower bound
        /// </summary>
        public float Lower;
        /// <summary>
        /// Upper bound
        /// </summary>
        public float Upper;

        /// <summary>
        /// True if the interval isn't empty
        /// </summary>
        public bool IsNonEmpty => Upper >= Lower;
        /// <summary>
        /// True if the interval is empty
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsEmpty => Upper < Lower;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        /// <summary>
        /// True if the interval is a single value
        /// </summary>
        public bool IsUnique => Upper == Lower;

        ///// <summary>
        ///// Lower the upper bound to the specified value, if it isn't already lower
        ///// </summary>
        ///// <param name="upper">New bound</param>
        //public void BoundAbove(float upper)
        //{
        //    if (upper < Upper)
        //        Upper = upper;
        //}

        ///// <summary>
        ///// Raise the lower bound to the specified value if it isn't already higher
        ///// </summary>
        ///// <param name="lower">New bound</param>
        //public void BoundBelow(float lower)
        //{
        //    if (lower > Lower)
        //        Lower = lower;
        //}
    }
}
