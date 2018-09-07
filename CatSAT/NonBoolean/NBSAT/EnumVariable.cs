#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumVariable.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace CatSAT
{
#pragma warning disable 660,661
    /// <summary>
    /// A finite-domain variable whose domain is a specified enumerated type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumVariable<T> : FDVariable<T>
#pragma warning restore 660,661
    {
        /// <summary>
        /// Creates a new EnumVariable.
        /// Domain need not be specified, because it's implicitly specified by the Enumerated type parameter of the class.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="condition">Condition under which the variable is defined (default always defined)</param>
        public EnumVariable(object name, Literal condition = null) : base(name, EnumDomain<T>.Singleton, condition)
        { }

        /// <summary>
        /// A Proposition asserting that the variable has a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator ==(EnumVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            // ReSharper disable once PossibleNullReferenceException
            return var.ValuePropositions[var.domain.IndexOf(value)];
        }

        /// <summary>
        /// A Proposition asserting that the variable does not have a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator !=(EnumVariable<T> var, T value)
        {
            Debug.Assert((object)var != null, nameof(var) + " != null");
            // ReSharper disable once PossibleNullReferenceException
            return Language.Not(var.ValuePropositions[var.domain.IndexOf(value)]);
        }
    }
}
