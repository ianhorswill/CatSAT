#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Biconditional.cs" company="Ian Horswill">
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
namespace CatSAT
{
    /// <summary>
    /// Constrains Head and Body to have the same truth value.
    /// </summary>
    public class Biconditional : Assertable
    {
        /// <summary>
        /// Literal stated to be equivalent to the body
        /// </summary>
        public readonly Literal Head;
        /// <summary>
        /// Literal or conjunction stated to be equivalent to the head
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// An expression stating that the head and body are equivalent (true in the same models)
        /// </summary>
        /// <param name="head">literal equivalent to the body</param>
        /// <param name="body">Literal or conjunction equivalent to the head</param>
        public Biconditional(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Head} == {Body}";
        }
        
        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }
    }
}