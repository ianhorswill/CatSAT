#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Menu.cs" company="Ian Horswill">
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

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    /// <summary>
    /// A fixed, finite collection of values for MenuVariable
    /// </summary>
    /// <typeparam name="T">Underlying type of the variable</typeparam>
    public class Menu<T> : Domain<T>
    {
        /// <summary>
        /// Create a Menu (domain of enumerated values) for a MenuVariable from a fixed set of values
        /// </summary>
        /// <param name="name">Name to give to the menu (for debugging purposes)</param>
        /// <param name="selections">Allowable values</param>
        public Menu(string name, T[] selections) : base(name)
        {
            Selections = selections;
        }

        /// <summary>
        /// Fixed set of values in the domain
        /// </summary>
        public readonly T[] Selections;

        /// <inheritdoc />
        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            return new MenuVariable<T>(name, this, p, condition);
        }
    }
}
