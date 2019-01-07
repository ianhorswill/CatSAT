#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VariableBound.cs" company="Ian Horswill">
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
namespace CatSAT.NonBoolean.SMT.Float
{
    /// <summary>
    /// Represents the constraint that one variable is greater than or equal to another
    /// </summary>
    class VariableBound : FloatProposition
    {
        public override void Initialize(Problem p)
        {
            base.Initialize(p);
            var c = (Call)Name;
            Lhs = (FloatVariable)c.Args[0];
            Rhs = (FloatVariable)c.Args[1];
        }

        /// <summary>
        /// The left-hand variable of hte constraint
        /// </summary>
        public FloatVariable Lhs;

        /// <summary>
        /// The right-hand variable of hte constraint
        /// </summary>
        public FloatVariable Rhs;

        /// <summary>
        /// The constraint is Lhs l.t.e. Rhs rather than the other way around
        /// </summary>
        public bool IsUpper => Operator == "<=";

        /// <summary>
        /// The relation of the constraint (e.g. less-than-equal)
        /// </summary>
        private string Operator => ((Call) Name).Name;
    }
}
