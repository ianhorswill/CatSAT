#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductConstraint.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// Represents the constraint that Result = lhs * rhs
    /// </summary>
    class ProductConstraint : FunctionalConstraint
    {
        /// <summary>
        /// The left-hand argument to *
        /// </summary>
        private FloatVariable lhs;
        /// <summary>
        /// The right-hand argument to *
        /// </summary>
        private FloatVariable rhs;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[0];
            lhs = (FloatVariable)c.Args[1];
            rhs = (FloatVariable)c.Args[2];
            lhs.AddFunctionalConstraint(this);
            rhs.AddFunctionalConstraint(this);
            base.Initialize(p);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (ReferenceEquals(changed, Result))
                return lhs.NarrowTo(Result.Bounds / rhs.Bounds, q)
                       && rhs.NarrowTo(Result.Bounds / lhs.Bounds, q);
            
            // It was lhs or rhs that changed.
            Debug.Assert(ReferenceEquals(changed, lhs) || ReferenceEquals(changed, rhs));
            FloatVariable other = ReferenceEquals(changed, lhs) ? rhs : lhs;
            return Result.NarrowTo(lhs.Bounds * rhs.Bounds, q)
                   && other.NarrowTo(Result.Bounds / changed.Bounds, q);
        }
    }
}
