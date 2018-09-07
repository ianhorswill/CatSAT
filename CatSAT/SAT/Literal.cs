#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Literal.cs" company="Ian Horswill">
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
using System;
using System.Diagnostics;

namespace CatSAT
{
    /// <summary>
    /// Represents a Proposition or a Negation.
    /// </summary>
#pragma warning disable 660,661
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
    public abstract class Literal : Expression
#pragma warning restore 660,661
    {
        /// <summary>
        /// Coerce a string to a literal
        /// Returns the proposition with the specified name
        /// </summary>
        /// <param name="s">Name of hte proposition</param>
        /// <returns>The proposition with that naem</returns>
        public static implicit operator Literal(string s)
        {
            return Proposition.MakeProposition(s);
        }

        /// <summary>
        /// Returns a biconditional rule asserting that the head and body are true in exactly the same models
        /// </summary>
        /// <param name="head">Literal that's equivalent to the body</param>
        /// <param name="body">Literal or conjunction that's equivalent to the head.</param>
        /// <returns></returns>
        public static Biconditional operator ==(Literal head, Expression body)
        {
            return new Biconditional(head, body);
        }

        /// <summary>
        /// This is not actually supported
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Biconditional operator !=(Literal head, Expression body)
        {
            throw new NotImplementedException("!= is undefined for CatSAT expressions");
        }

        /// <summary>
        /// The position of this literal in the Variables array of this literal's Problem.
        /// This is also the position of its truth value in the propositions array of a Solution to the Problem.
        /// IMPORTANT: if this is a negation, then the index is negative.  That is, if p has index 2, then
        /// Not(p) has index -2.
        /// </summary>
        internal abstract short SignedIndex { get; }

        internal override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
        {
            clauseArray[startingPosition] = (short)-SignedIndex;
            return startingPosition + 1;
        }

        /// <summary>
        /// This syntax is not supported for implications or rules.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Implication operator <(Expression body, Literal head)
        {
            throw new NotImplementedException("Use head <= body for rules");
        }

        /// <summary>
        /// Returns an implication asserting that the body implied the head, but not vice-versa
        /// </summary>
        /// <param name="body">A literal or conjunction of literals</param>
        /// <param name="head">A literal implied by the body.</param>
        /// <returns></returns>
        public static Implication operator >(Expression body, Literal head)
        {
            return new Implication(head, body);
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }

        /// <summary>
        /// Coerces a boolean to a Proposition with a fixed truth value
        /// </summary>
        /// <param name="b">Boolean</param>
        /// <returns>Proposition - either Proposition.True or Proposition.False</returns>
        public static implicit operator Literal(bool b)
        {
            return (Proposition) b;
        }

        private string DebugName => ToString();
    }
}