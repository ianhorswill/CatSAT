#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonotoneFunction.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    internal class MonotoneFunctionConstraint : FunctionalConstraint
    {
        public static FloatVariable MonotoneFunction(string name, Func<float, float> function, Func<float, float> inverseFunction, bool increasing, FloatVariable input)
        {
            var lower = function(input.Bounds.Lower);
            var upper = function(input.Bounds.Upper);
            var output = new FloatVariable($"{name}({input.Name})", increasing ? lower : upper,
                increasing ? upper : lower, input.Condition) {PickLast = true};
            AddConstraint(name, function, inverseFunction, increasing, input, output);
            return output;
        }

        static void AddConstraint(string name, Func<float, float> function, Func<float, float> inverseFunction, bool increasing, FloatVariable input, FloatVariable output)
        {
            var constraint = Problem.Current.GetSpecialProposition<MonotoneFunctionConstraint>(Call.FromArgs(Problem.Current, name+"!", input, output));
            constraint.function = function;
            constraint.inverseFunction = inverseFunction;
            constraint.increasing = increasing;
            Problem.Current.Assert(constraint);
        }

        /// <summary>
        /// The input to the function
        /// </summary>
        private FloatVariable input;

        private Func<float, float> function;
        private Func<float, float> inverseFunction;
        private bool increasing;

        public override void Initialize(Problem p)
        {
            var c = (Call)Name;
            Result = (FloatVariable)c.Args[1];
            input = (FloatVariable)c.Args[0];
            input.AddFunctionalConstraint(this);
            Result.PickLast = true;
            base.Initialize(p);
        }

        public override bool Propagate(FloatVariable changed, bool isUpper, Queue<Tuple<FloatVariable,bool>> q)
        {
            if (ReferenceEquals(changed, Result))
            {
                if (isUpper)
                    // Upper bound of Result decreased
                {
                    var inverse = inverseFunction(Result.Bounds.Upper);
                    return increasing ? input.BoundAbove(inverse) : input.BoundBelow(inverse);
                }
                else
                    // Lower bound of result increased
                {
                    var inverse = inverseFunction(Result.Bounds.Lower);
                    return increasing ? input.BoundBelow(inverse) : input.BoundAbove(inverse);
                }
            }
            else
            {
                Debug.Assert(ReferenceEquals(changed, input));
                if (isUpper)
                    // Upper bound of input decreased
                {
                    var res = function(input.Bounds.Upper);
                    return increasing ? Result.BoundAbove(res) : Result.BoundBelow(res);
                }
                else
                    // Lower bound of input increased
                {
                    var res = function(input.Bounds.Lower);
                    return increasing ? Result.BoundBelow(res) : Result.BoundAbove(res);
                }
            }
        }

        public override bool IsDefinedIn(Solution s)
        {
            return s[this] && Result.IsDefinedInInternal(s) && input.IsDefinedInInternal(s);
        }
    }
}
