#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatDomain.cs" company="Ian Horswill">
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
        /// If non-zero, domain consists of only integer multiples of quantization.
        /// </summary>
        public float Quantization;

        /// <summary>
        /// True if num is a valid element of the domain.
        /// </summary>
        public bool Contains(float num)
        {
            return ((Bounds.Contains(num)) && (Quantization == 0 || IsQuantized(num, Quantization)));
        }

        /// <summary>
        /// True if num is an integer multiple of quantization.
        /// </summary>
        public bool IsQuantized(float num, float quantization)
        {
            var quotient = num / quantization;
            var rounded = Math.Round(quotient);
            var error = Math.Abs(quotient - rounded);
            return (error < 0.0001f);
        }

        /// <summary>
        /// A floating-point domain, specified by upper- and lower-bounds
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="lowerBound">Lower bound of the variable</param>
        /// <param name="upperBound">Upper bound of the variable</param>
        /// <param name="quantization">If non-zero, include only integer multiples of quantization</param>
        public FloatDomain(string name, float lowerBound, float upperBound, float quantization) : base(name)
        {
            Bounds = new Interval(lowerBound, upperBound);
            Quantization = quantization;
        }

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

        /// <summary>
        /// A FloatDomain constrained to be the sum of two other FloatDomains
        /// </summary>
        public static FloatDomain operator +(FloatDomain d1, FloatDomain d2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (d1.Quantization != d2.Quantization)
            {
                throw new ArgumentException("Can't add float domains with different quantizations");
            }

            else if (d1.Quantization == 0 && d2.Quantization == 0)
            {
                var sum = new FloatDomain($"{d1.Name}+{d2.Name}", d1.Bounds.Lower + d2.Bounds.Lower,
                    d1.Bounds.Upper + d2.Bounds.Upper);
                return sum;
            }

            else
            {
                var sum = new FloatDomain($"{d1.Name}+{d2.Name}", d1.Bounds.Lower + d2.Bounds.Lower,
                    d1.Bounds.Upper + d2.Bounds.Upper) {Quantization = d1.Quantization};
                return sum;
            }
        }

        /// <summary>
        /// A FloatDomain constrained to be the difference of two other FloatDomains
        /// </summary>
        public static FloatDomain operator -(FloatDomain d1, FloatDomain d2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (d1.Quantization != d2.Quantization)
            {
                throw new ArgumentException("Can't subtract float domains with different quantizations");
            }

            else if (d1.Quantization == 0 && d2.Quantization == 0)
            {
                var difference = new FloatDomain($"{d1.Name}-{d2.Name}", d1.Bounds.Lower - d2.Bounds.Upper,
                d1.Bounds.Upper - d2.Bounds.Lower);
                return difference;
            }

            else
            {
                var difference = new FloatDomain($"{d1.Name}-{d2.Name}", d1.Bounds.Lower - d2.Bounds.Upper,
                    d1.Bounds.Upper - d2.Bounds.Lower) {Quantization = d1.Quantization};
                return difference;
            }
        }

        /// <summary>
        /// A FloatDomain constrained to be the product of two other FloatDomains
        /// </summary>
        public static FloatDomain operator *(FloatDomain d1, FloatDomain d2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (d1.Quantization != d2.Quantization)
            {
                throw new ArgumentException("Can't multiply float domains with different quantizations");
            }

            else if (d1.Quantization == 0 && d2.Quantization == 0)
            {

                var product = new FloatDomain($"{d1.Name}*{d2.Name}", d1.Bounds.Lower * d2.Bounds.Lower,
                    d1.Bounds.Upper * d2.Bounds.Upper);
                return product;
            }

            else
            {
                var product = new FloatDomain($"{d1.Name}*{d2.Name}", d1.Bounds.Lower * d2.Bounds.Lower,
                    d1.Bounds.Upper * d2.Bounds.Upper) {Quantization = d1.Quantization};
                return product;
            }
        }



    }
}
