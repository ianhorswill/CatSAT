#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Conjunction.cs" company="Ian Horswill">
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
using System.Collections.Generic;

namespace CatSAT
{
    /// <summary>
    /// A conjunction of literals.
    /// Used in rule bodies.
    /// </summary>
    public class Conjunction : Expression
    {
        /// <summary>
        /// LHS of the conjunction
        /// </summary>
        public readonly Expression Left;
        /// <summary>
        /// RHS of the conjunction
        /// </summary>
        public readonly Expression Right;

        /// <summary>
        /// An expression representing the condition in which both left and right are true
        /// </summary>
        /// <param name="left">LHS of the conjunction</param>
        /// <param name="right">RHS of the conjunction</param>
        public Conjunction(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Left} & {Right}";
        }

        /// <summary>
        /// Number of terms in the conjunction
        /// </summary>
        public override int Size => Left.Size + Right.Size;

        internal override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
        {
            return Right.WriteNegatedSignedIndicesTo(clauseArray, Left.WriteNegatedSignedIndicesTo(clauseArray, startingPosition));
        }
        
        internal override void Assert(Problem p)
        {
            Left.Assert(p);
            Right.Assert(p);
        }

        internal override IEnumerable<Proposition> PositiveLiterals
        {
            get
            {
                foreach (var p in Left.PositiveLiterals)
                    yield return p;

                foreach (var p in Right.PositiveLiterals)
                    yield return p;
            }
        }
    }
}