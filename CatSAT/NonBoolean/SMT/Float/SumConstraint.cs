#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SumConstraint.cs" company="Ian Horswill">
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
    /// Represents the constraint that Result = lhs + rhs
    /// </summary>
    class SumConstraint : FunctionalConstraint
    {
        /// <summary>
        /// The left-hand argument to +
        /// </summary>
        private FloatVariable lhs;
        /// <summary>
        /// The right-hand argument to +
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
            {
                if (isUpper)
                    // Upper bound of Result decreased
                    // Result = lhs+rhs, so lhs = Result-rhs, rhs = result-lhs
                    return lhs.BoundAbove(Result.Bounds.Upper - rhs.Bounds.Lower, q)
                           && rhs.BoundAbove(Result.Bounds.Upper - lhs.Bounds.Lower, q);

                // Lower bound of Result increased
                return lhs.BoundBelow(Result.Bounds.Lower - rhs.Bounds.Upper, q)
                       && rhs.BoundBelow(Result.Bounds.Lower - lhs.Bounds.Upper, q);
            }
            else
            {
                Debug.Assert(ReferenceEquals(changed,lhs) || ReferenceEquals(changed, rhs));
                FloatVariable other = ReferenceEquals(changed, lhs) ? rhs : lhs;
                if (isUpper)
                    // Upper bound decreased
                    return Result.BoundAbove(lhs.Bounds.Upper + rhs.Bounds.Upper, q)
                           && other.BoundBelow(Result.Bounds.Lower - changed.Bounds.Upper, q);

                // Lower bound increased
                return Result.BoundBelow(lhs.Bounds.Lower + rhs.Bounds.Lower, q)
                       && other.BoundAbove(Result.Bounds.Upper - changed.Bounds.Lower, q);
            }
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedIn(s) && lhs.IsDefinedIn(s) && rhs.IsDefinedIn(s);
        }
    }
}
