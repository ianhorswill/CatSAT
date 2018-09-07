#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionAttribute.cs" company="Ian Horswill">
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
    /// An attribute representing that the Variable to which it is attached is only defined in solutions for which the specified variable has the specified value.
    /// </summary>
    public class ConditionAttribute : Attribute
    {
        /// <summary>
        /// Variable whose value to check
        /// </summary>
        public readonly string VariableName;
        /// <summary>
        /// Value to compare to
        /// </summary>
        public readonly object VariableValue;

        /// <summary>
        /// States that this variable is only defined when variableName's value is equal to variableValue
        /// </summary>
        /// <param name="variableName">Variable to check the value of</param>
        /// <param name="variableValue">Value the variable should have</param>
        public ConditionAttribute(string variableName, object variableValue)
        {
            VariableName = variableName;
            VariableValue = variableValue;
        }
    }
}
