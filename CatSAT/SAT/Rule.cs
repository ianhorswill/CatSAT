#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Ian Horswill">
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
    /// States that Body is a possible justification for the truth of Head.
    /// This is different from an implication.  An implication says that Body being true forces Head to be true
    /// and nothing more.  A rule gives the Head completion semantics.  It says that the Body being true forces 
    /// the Head to be true, but also says that the Head being true forces at least one of its rule bodies to be
    /// true.
    /// </summary>
    public class Rule : Assertable
    {
        /// <summary>
        /// The proposition being concluded by the rule
        /// </summary>
        public readonly Proposition Head;
        /// <summary>
        /// The literal or conjunction that would justify concluding the head.
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// A rule that states that the head is justified by the body
        /// </summary>
        /// <param name="head">proposition that can be concluded from the body</param>
        /// <param name="body">Literal or conjunction that would justify the head</param>
        public Rule(Proposition head, Expression body)
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