#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SumConstraint.cs" company="Ian Horswill">
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
using System.Linq;

namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// Represents the constraint that Result = lhs + rhs
    /// </summary>
    class GeneralSumConstraint : FunctionalConstraint
    {
        /// <summary>
        /// The left-hand argument to +
        /// </summary>
        private FloatVariable[] addends;

        private float scaleFactor = 1;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[0];
            scaleFactor = (float) c.Args[1];
            addends = (FloatVariable[]) c.Args[2];
            foreach (var a in addends)
                a.AddFunctionalConstraint(this);
            base.Initialize(p);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable, bool>> q)
        {
            var addendSum = Interval.Zero;
            foreach (var a in addends)
                addendSum += a.Bounds;

            if (ReferenceEquals(changed, Result))
            {
                if (isUpper)
                {
                    var resultBound = Result.Bounds.Upper / scaleFactor;
                    foreach (var a in addends)
                        if (!a.BoundAbove(resultBound - (addendSum.Lower - a.Bounds.Lower)))
                            return false;
                }
                else
                {
                    var resultBound = Result.Bounds.Lower / scaleFactor;
                    foreach (var a in addends)
                        if (!ReferenceEquals(a, changed) &&
                            !a.BoundBelow(resultBound - (addendSum.Upper - a.Bounds.Upper)))
                            return false;
                }
            }
            else
            {
                if (isUpper)
                {
                    var resultBound = Result.Bounds.Lower / scaleFactor;
                    if (!Result.BoundAbove(scaleFactor * addendSum.Upper))
                        return false;
                    foreach (var a in addends)
                        if (!ReferenceEquals(a, changed) &&
                            !a.BoundBelow(resultBound - (addendSum.Upper - a.Bounds.Upper)))
                            return false;
                }
                else
                {
                    var resultBound = Result.Bounds.Upper / scaleFactor;
                    if (!Result.BoundBelow(scaleFactor * addendSum.Lower))
                        return false;
                    foreach (var a in addends)
                        if (!ReferenceEquals(a, changed) &&
                            !a.BoundAbove(resultBound - (addendSum.Lower - a.Bounds.Lower)))
                            return false;
                }
            }

            return true;
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedIn(s) && addends.All(a => a.IsDefinedIn(s));
        }
    }
}
