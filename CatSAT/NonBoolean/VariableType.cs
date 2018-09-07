#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VariableType.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace CatSAT
{
    /// <summary>
    /// Base class of all "types" of CatSAT variables.
    /// VariableType is a generalization of domains that includes things like the types of CatSAT Structs,
    /// that don't really look like domains.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract class VariableType
    {
        /// <summary>
        /// Name of the type, for debugging purposes
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Dictionary of all named types
        /// </summary>
        public static readonly Dictionary<string, VariableType> Types = new Dictionary<string, VariableType>();

        /// <summary>
        /// Make a VariableType and add it to the Types dictionary.
        /// </summary>
        /// <param name="name"></param>
        protected VariableType(string name)
        {
            Name = name;
            Types[Name] = this;
        }

        /// <summary>
        /// Find the CatSAT VariableType object with the specified name
        /// </summary>
        /// <param name="n">Name to look for</param>
        public static VariableType TypeNamed(string n)
        {
            return Types[n];
        }

        /// <summary>
        /// True if there is a CatSAT VariableType with this name
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool TypeExists(string n)
        {
            return Types.ContainsKey(n);
        }

        /// <summary>
        /// Create a new Variable object with this VariableType within the specified Problem.
        /// </summary>
        /// <param name="name">Name to give to the variable</param>
        /// <param name="p">Problem to add the variable to</param>
        /// <param name="condition">Optional condition for when the variable should be defined in a given solution.</param>
        public abstract Variable Instantiate(object name, Problem p, Literal condition = null);

        /// <summary>
        /// Create a new Variable object with this VariableType within the specified Problem.
        /// </summary>
        /// <param name="name">Name to give to the variable</param>
        public Variable Instantiate(object name)
        {
            return Instantiate(name, Problem.Current);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
