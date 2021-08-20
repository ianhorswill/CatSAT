using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// Represents the constraint that Result = num ^ exponent
    /// </summary>
    class PowerConstraint : FunctionalConstraint
    {
        /// <summary>
        /// The number to be raised to a power.
        /// </summary>
        private FloatVariable num;

        /// <summary>
        /// The exponent a number is raised to.
        /// </summary>
        private uint exponent;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[0];
            num = (FloatVariable)c.Args[1];
            exponent = (uint)c.Args[2];
            num.AddFunctionalConstraint(this);
            Result.PickLast = true;
            base.Initialize(p);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable, bool>> q)
        {
            if (ReferenceEquals(changed, Result))
            {
                //Result has been altered
                //Result = num ^ exponent, and because PowerConstraint only handles bounds that cross zero even-numbered exponents,
                //the negative and positive values of the largest possible Result's inverse function are the bounds of num. 
                var bound = (float)Math.Pow(Result.Bounds.Upper, 1 / exponent);
                
                return num.NarrowTo(new Interval(-bound, bound), q);
            }

            return Result.NarrowTo(num.Bounds^exponent, q);
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedInInternal(s) && num.IsDefinedInInternal(s);
        }
    }
}