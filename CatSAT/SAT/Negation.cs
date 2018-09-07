#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Negation.cs" company="Ian Horswill">
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
    /// Represents a negated proposition.
    /// </summary>
    public class Negation : Literal
    {
        /// <summary>
        /// The proposition being negated
        /// </summary>
        public readonly Proposition Proposition;

        /// <summary>
        /// Creates a Literal representing the negation of the proposition
        /// </summary>
        /// <param name="proposition">Proposition to negate</param>
        public Negation(Proposition proposition)
        {
            Proposition = proposition;
        }
    
        /// <summary>
        /// Creates a Literal representing the negation of the proposition
        /// </summary>
        /// <param name="p">Proposition to negate</param>
        public static Literal Not(Proposition p)
        {
            if (p.IsConstant)
            {
                return (bool)p ? Proposition.False : Proposition.True;
            }
            return Problem.Current.Negation(p);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"!{Proposition}";
        }

        internal override short SignedIndex => (short)-Proposition.Index;

        internal override IEnumerable<Proposition> PositiveLiterals
        {
            get
            {
                // This is a negative literal, not a positive one.
                yield break;
            }
        }
    }
}