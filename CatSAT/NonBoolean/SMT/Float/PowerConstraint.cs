using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    class PowerConstraint : FunctionalConstraint
    {
        private FloatVariable num;

        private uint exponent;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[0];
            num = (FloatVariable)c.Args[1];
            exponent = (uint)c.Args[2];
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

            return Result.NarrowTo(num.Bounds^exponent, q);
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedInInternal(s) && num.IsDefinedInInternal(s);
        }
    }
}