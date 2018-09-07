#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Call.cs" company="Ian Horswill">
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
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// Represents a call to a predicate or function with specific arguments.
    /// This gets used as the name of the Proposition that the predicate returns when you call it.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class Call
    {
        /// <summary>
        /// Name of the predicate or other functor being called
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Arguments
        /// </summary>
        public readonly object[] Args;

        /// <summary>
        /// Makes a new "call" object.  This is just an object used to fill in the name field for a proposition that conceptually
        /// represents the truth of some predicate with some specific arguments
        /// </summary>
        /// <param name="name">Name of the predicate or other functor</param>
        /// <param name="args">Arguments</param>
        public Call(string name, params object[] args)
        {
            Name = name;
            Args = args;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = Name.GetHashCode();
            foreach (var a in Args)
                hash ^= a.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Call c)
            {
                if (Name != c.Name)
                    return false;
                if (Args.Length != c.Args.Length)
                    return false;
                for (int i = 0; i < Args.Length; i++)
                    if (!Args[i].Equals(c.Args[i]))
                        return false;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Name);
            b.Append('(');
            var firstOne = true;
            foreach (var a in Args)
            {
                if (firstOne)
                    firstOne = false;
                else
                    b.Append(", ");
                b.Append(a);
            }

            b.Append(')');
            return b.ToString();
        }

        private string DebuggerDisplay => ToString();
    }
}
