#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FDomain.cs" company="Ian Horswill">
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
using System.Collections.Generic;

namespace CatSAT
{
    /// <summary>
    /// A finite domain defined by a list.
    /// </summary>
    /// <typeparam name="T">Base type of the domain</typeparam>
    public class FDomain<T> : Domain<T>
    {
        /// <summary>
        /// The values of the domain
        /// </summary>
        public readonly IList<T> Elements;

        /// <summary>
        /// A finite domain with the specified name and domain elements
        /// </summary>
        /// <param name="name">NAme to give to the domain</param>
        /// <param name="elements">Elements of hte domain</param>
        public FDomain(string name, params T[] elements) : this(name, (IList<T>)elements)
        { }

        /// <summary>
        /// A finite domain with the specified name and domain elements
        /// </summary>
        /// <param name="name">NAme to give to the domain</param>
        /// <param name="elements">Elements of hte domain</param>
        public FDomain(string name, IList<T> elements) : base(name)
        {
            Elements = elements;
        }

        /// <summary>
        /// Element number of the specified domain element.
        /// Elements are numbered 0, 1, ... #elements-1.
        /// </summary>
        /// <param name="element">Desired domain element</param>
        /// <returns>Index of element within the Elements list</returns>
        /// <exception cref="ArgumentException">If argument isn't a valid domain element</exception>
        public int IndexOf(T element)
        {
            var index = Elements.IndexOf(element);
            if (index < 0)
                throw new ArgumentException($"{element} is not an element of the domain {Name}");
            return index;
        }

        /// <summary>
        /// Returns the i'th element of the domain.  Elements are numbered from 0.
        /// </summary>
        /// <param name="i">Element number</param>
        public T this[int i] => Elements[i];

        /// <inheritdoc />
        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            // ReSharper disable once ObjectCreationAsStatement
            return new FDVariable<T>(name, this, condition);
        }
    }
}
