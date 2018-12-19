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
        private readonly FloatVariable lhs;
        /// <summary>
        /// The right-hand argument to +
        /// </summary>
        private readonly FloatVariable rhs;

        public SumConstraint(FloatVariable result, FloatVariable lhs, FloatVariable rhs)
            : base(result)
        {
            this.lhs = lhs;
            this.rhs = rhs;
            lhs.AddFunctionalConstraint(this);
            rhs.AddFunctionalConstraint(this);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (ReferenceEquals(changed, Result))
            {
                if (isUpper)
                {
                    // Upper bound of Result decreased
                    // Result = lhs+rhs, so lhs = Result-rhs, rhs = result-lhs
                    if (!lhs.BoundAbove(Result.Bounds.Upper - rhs.Bounds.Lower, q))
                        return false;
                    if (!rhs.BoundAbove(Result.Bounds.Upper - lhs.Bounds.Lower, q))
                        return false;
                }
                else
                {
                    // Lower bound of Result increased
                    if (!lhs.BoundBelow(Result.Bounds.Lower - rhs.Bounds.Upper, q))
                        return false;
                    if (!rhs.BoundBelow(Result.Bounds.Lower - lhs.Bounds.Upper, q))
                        return false;
                }
            }
            else
            {
                Debug.Assert(ReferenceEquals(changed,lhs) || ReferenceEquals(changed, rhs));
                FloatVariable other = ReferenceEquals(changed, lhs) ? rhs : lhs;
                if (isUpper)
                {
                    // Upper bound decreased
                    if (!Result.BoundAbove(lhs.Bounds.Upper + rhs.Bounds.Upper, q))
                        return false;
                    if (!other.BoundBelow(Result.Bounds.Lower - changed.Bounds.Upper, q))
                        return false;
                }
                else
                {
                    // Lower bound increased
                    if (!Result.BoundBelow(lhs.Bounds.Lower + rhs.Bounds.Lower, q))
                        return false;
                    if (!other.BoundAbove(Result.Bounds.Upper - changed.Bounds.Lower, q))
                        return false;
                }
            }
            
            return true;
        }
    }
}
