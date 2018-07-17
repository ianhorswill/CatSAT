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

namespace PicoSAT
{
    /// <summary>
    /// A finite domain defined by a list.
    /// </summary>
    /// <typeparam name="T">Base type of the domain</typeparam>
    public class FDomain<T> : Domain<T>
    {
        public readonly IList<T> Values;

        public FDomain(string name, params T[] values) : this(name, (IList<T>)values)
        { }

        public FDomain(string name, IList<T> values) : base(name)
        {
            Values = values;
        }

        public int IndexOf(T value)
        {
            var index = Values.IndexOf(value);
            if (index < 0)
                throw new ArgumentException($"{value} is not an element of the domain {Name}");
            return index;
        }

        public T this[int i] => Values[i];

        public override Variable Instantiate(object name, Problem p, Literal condition = null)
        {
            // ReSharper disable once ObjectCreationAsStatement
            return new FDVariable<T>(name, this, condition);
        }
    }
}
