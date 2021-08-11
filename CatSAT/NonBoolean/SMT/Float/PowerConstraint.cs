using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    class PowerConstraint : FunctionalConstraint
    {
        private FloatVariable num;

        private float exponent;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[0];
            num = (FloatVariable)c.Args[1];
            exponent = (int)c.Args[2];
            num.AddFunctionalConstraint(this);
            base.Initialize(p);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable, bool>> q)
        {
            if (ReferenceEquals(changed, Result))
            {
                float lower, upper;
                if (Result.Bounds.Upper < 0)
                {
                    upper = (float)-Math.Pow(-Result.Bounds.Upper, 1 / exponent);
                }

                else
                {
                    upper = (float)Math.Pow(Result.Bounds.Upper, 1 / exponent);
                }

                if (Result.Bounds.Lower < 0)
                {
                    lower = (float)-Math.Pow(-Result.Bounds.Lower, 1 / exponent);
                }
                else
                {
                    lower = (float)Math.Pow(Result.Bounds.Lower, 1 / exponent);
                }
                var invBounds = new Interval(lower, upper);
                return num.NarrowTo(invBounds, q);
            }

            if (num.Bounds.Upper <= 0)
            {
                var lower = (float)-Math.Pow(Math.Max(0, -num.Bounds.Lower), 1 / exponent);
                var upper = (float)-Math.Pow(Math.Max(0, -num.Bounds.Upper), 1 / exponent);
                var invInterval = new Interval(-lower, -upper);

                return num.NarrowTo(invInterval, q);
            }
            else
            {
                var bound = (float)Math.Pow(num.Bounds.Upper, 1 / exponent);
                var invInterval = new Interval(-bound, bound);
                return num.NarrowTo(invInterval, q);
            }
            //check if ReferenceEquals(changed, Result) is true;
            //return the result of num.NarrowTo using inverse function as a parameter

            //Narrow Result's bounds to bounds raised to the exponent.

            // To do this, since it's an even exponent:
            // If num is non-positive, narrow to the negative interval returned by the inverse function.

            // Else, perform the inverse function and retrieve the Upper value.
            // Narrow to a new interval between the negative and positive of that value.
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedInInternal(s) && num.IsDefinedInInternal(s);
        }
    }
}