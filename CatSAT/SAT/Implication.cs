#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Implication.cs" company="Ian Horswill">
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
namespace CatSAT
{
    /// <summary>
    /// States that the Body being true forces the Head to be true without placing any constraint on Body.
    /// </summary>
    public class Implication : Assertable
    {
        /// <summary>
        /// The literal implied by the assertion
        /// </summary>
        public readonly Literal Head;
        /// <summary>
        /// The condition under which the head is implied
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// Creates an expression representing that a given Literal or conjunction of literals implies the specifed literal.
        /// </summary>
        /// <param name="head">literal implied by the body</param>
        /// <param name="body">literal or conjunction that implies the head</param>
        public Implication(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Head} <= {Body}";
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }
    }
}