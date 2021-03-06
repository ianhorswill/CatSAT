﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantBound.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// The constraint that a variable is bounded above/below by a constant
    /// The specific constraint is: Variable Operator Bound
    /// </summary>
    [DebuggerDisplay("{Variable.Name} {Operator} {Bound}")]
    class ConstantBound : FloatProposition
    {
        public override void Initialize(Problem p)
        {
            base.Initialize(p);
            var c = (Call)Name;
            Variable = (FloatVariable)c.Args[0];
            Bound = (float)c.Args[1];
            (IsUpper?Variable.UpperConstantBounds:Variable.LowerConstantBounds).Add(this);
        }

        /// <summary>
        /// The variable being bounded
        /// </summary>
        public FloatVariable Variable;
        /// <summary>
        /// The value bounding the variable
        /// </summary>
        public float Bound;
        /// <summary>
        /// This is an upper bound on the variable
        /// </summary>
        public bool IsUpper => Operator == "<=";

        /// <summary>
        /// The relation between the Variable, on the left, and the Bound on the right.
        /// </summary>
        private string Operator => ((Call) Name).Name;
    }
}
