#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Variable.cs" company="Ian Horswill">
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

namespace PicoSAT
{
    /// <summary>
    /// Base class for non-Boolean variables
    /// </summary>
#pragma warning disable 660,661
    public abstract class Variable
#pragma warning restore 660,661
    {
        /// <summary>
        /// Object that names the variable
        /// </summary>
        public readonly object Name;

        /// <summary>
        /// Problem to which this variable is attached
        /// </summary>
        public readonly Problem Problem;
        /// <summary>
        /// Condition under which this variable is defined.
        /// If true or if Condition is null, the variable must have a value.
        /// If false, it must not have a value.
        /// </summary>
        public readonly Literal Condition;

        /// <summary>
        /// Make a new variable
        /// </summary>
        /// <param name="name">"Name" for the variable (arbitrary object)</param>
        /// <param name="problem">Problem of which this variable is a part</param>
        /// <param name="condition">Condition under which this variable is to have a value; if null, it's always defined.</param>
        protected Variable(object name, Problem problem, Literal condition)
        {
            Name = name;
            Problem = problem;
            Condition = condition;
            problem.AddVariable(this);
        }

        public virtual string ValueString(Solution s)
        {
            if (IsDefinedIn(s))
                return $"{Name}={UntypedValue(s)}";
            return $"{Name} undefined";
        }

        public abstract object UntypedValue(Solution s);

        public abstract bool IsDefinedIn(Solution solution);

        public override string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// A Proposition asserting that the variable has a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator ==(Variable var, object value)
        {
            return var.EqualityProposition(value);
        }

        /// <summary>
        /// A Proposition asserting that the variable does not have a specified value.
        /// </summary>
        /// <param name="var">The variable who value should be checked</param>
        /// <param name="value">The value to check for</param>
        /// <returns></returns>
        public static Literal operator !=(Variable var, object value)
        {
            return Language.Not(var == value);
        }



        public virtual Literal EqualityProposition(object vConditionValue)
        {
            throw new NotImplementedException();
        }
    }
}
