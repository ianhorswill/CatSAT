#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainAttribute.cs" company="Ian Horswill">
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

namespace CatSAT
{
    /// <summary>
    /// Annotates a Variable-valued field in a CompiledStruct to specify its domain
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DomainAttribute : Attribute
    {
        /// <summary>
        /// Make of this variable's domain
        /// </summary>
        public readonly string DomainName;

        /// <summary>
        /// Specify that this field should have the specified domain.
        /// Field must appear in a CompiledStruct and must be a subtype of CatSAT.Variable
        /// </summary>
        /// <param name="domainName"></param>
        // ReSharper disable once UnusedMember.Global
        public DomainAttribute(string domainName)
        {
            DomainName = domainName;
        }

        /// <summary>
        /// Specify that this field should have the specified finite domain with the specified elements.
        /// Field must appear in a CompiledStruct and must be a subtype of CatSAT.Variable
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="domainElements">Elements of the domain</param>
        public DomainAttribute(string domainName, params string[] domainElements)
        {
            DomainName = domainName;
            if (!VariableType.TypeExists(domainName))
                // ReSharper disable once ObjectCreationAsStatement
                new FDomain<string>(domainName, domainElements);
        }

        /// <summary>
        /// Domain being specified.
        /// </summary>
        public VariableType Domain => VariableType.TypeNamed(DomainName);
    }
}
