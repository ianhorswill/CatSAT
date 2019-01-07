#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Expression.cs" company="Ian Horswill">
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
using System.Collections.Generic;

namespace CatSAT
{
    /// <summary>
    /// A propositional expression
    /// </summary>
    public abstract class Expression : Assertable
    {
        /// <summary>
        /// Number of literals in the expression
        /// </summary>
        public virtual int Size => 1;

        /// <summary>
        /// Walk the expression tree and write the indicies of its literals into the specified array.
        /// </summary>
        /// <param name="clauseArray">Array of signed indices for the clause we're writing this to.</param>
        /// <param name="startingPosition">Position to start writing</param>
        /// <returns></returns>
        internal abstract int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition);

        /// <summary>
        /// Make a conjunction of two Expressions.
        /// Performs simple constant folding, e.g. false and p = False, true and p = p.
        /// </summary>
        public static Expression operator &(Expression left, Expression right)
        {
            if (left is Proposition l && l.IsConstant)
                    return (bool)l ? right : Proposition.False;
            if (right is Proposition r && r.IsConstant)
                return (bool)r ? left : Proposition.False;
            return new Conjunction(left, right);
        }

        /// <summary>
        /// Coerce an expression to a boolean
        /// Will throw an exception unless the expression is really a constant-valued Proposition.
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator Expression(bool b)
        {
            return (Proposition)b;
        }

        /// <summary>
        /// Find all the non-negated propositions in this Expression.
        /// </summary>
        internal abstract IEnumerable<Proposition> PositiveLiterals { get; }
    }
}
