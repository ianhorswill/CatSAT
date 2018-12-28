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
using System;
using System.Diagnostics;

namespace CatSAT
{
    /// <summary>
    /// A closed interval over the single-precision floating-point numbers
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
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
        /// An interval representing all possible float values
        /// </summary>
        public static readonly Interval AllValues = new Interval(float.NegativeInfinity, float.PositiveInfinity);

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

        /// <summary>
        /// True if interval includes positive, negative, and zero values
        /// </summary>
        public bool CrossesZero => Lower < 0 && Upper > 0;

        /// <summary>
        /// Interval contains value
        /// </summary>
        public bool Contains(float value)
        {
            return Lower <= value && value <= Upper;
        }

        /// <summary>
        /// The interval product of two intervals
        /// </summary>
        public static Interval operator*(Interval a, Interval b)
        {
            return new Interval(ProductMin(a, b), ProductMax(a, b));
        }

        /// <summary>
        /// The interval quotient of two intervals.
        /// IMPORTANT: if the denominator crosses zero, the set-theoretic quotient is not an interval.
        /// This operator then returns the whole real line as the interval.  Sorry.
        /// </summary>
        public static Interval operator/(Interval a, Interval b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (b.Lower == 0)
            {
                if (b.Upper == 0)
                    return AllValues;
                return new Interval(Math.Min(a.Upper / b.Upper, a.Lower / b.Upper), float.PositiveInfinity);
            }

            if (b.Upper == 0)
                // ReSharper restore CompareOfFloatsByEqualityOperator
                return new Interval(float.NegativeInfinity, Math.Max(a.Lower / b.Lower, a.Upper / b.Lower));

            if (b.Contains(0))
                return AllValues;

            return new Interval(
                Min(a.Lower / b.Lower, a.Upper / b.Upper, a.Lower / b.Upper, a.Upper / b.Lower),
                Max(a.Lower / b.Lower, a.Upper / b.Upper, a.Lower / b.Upper, a.Upper / b.Lower));
        }

        static float ProductMin(Interval a, Interval b)
        {
            return Min(a.Upper * b.Upper, a.Upper * b.Lower, a.Lower * b.Upper, a.Lower * b.Lower);
        }

        static float ProductMax(Interval a, Interval b)
        {
            return Max(a.Upper * b.Upper, a.Upper * b.Lower, a.Lower * b.Upper, a.Lower * b.Lower);
        }

        static float Min(float a, float b, float c, float d)
        {
            return Math.Min(Math.Min(a, b), Math.Min(c, d));
        }

        static float Max(float a, float b, float c, float d)
        {
            return Math.Max(Math.Max(a, b), Math.Max(c, d));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Lower},{Upper}]";
        }

        private string DebuggerDisplay => ToString();
    }
}
