#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuVariable.cs" company="Ian Horswill">
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
using System;
using System.Collections.Generic;

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    /// <summary>
    /// A theory variable whose possible values come from a fixed, finite domain of specified values
    /// </summary>
    /// <typeparam name="T">Underlying type for the variable's values</typeparam>
    public class MenuVariable<T> : TheoryVariable<T>
    {
        /// <summary>
        /// Make a variable whose allowable values from some the specified menu
        /// </summary>
        /// <param name="name">Name of the variable, for debugging purposes</param>
        /// <param name="baseMenu">Default menu of allowable values</param>
        /// <param name="problem">Problem object to which this variable belongs</param>
        /// <param name="condition">Literal that must be true within a given solution in order for this variable to be defined in that solution</param>
        public MenuVariable(object name, Menu<T> baseMenu, Problem problem, Literal condition = null) : base(name, problem, condition)
        {
            BaseMenu = baseMenu;
            problem.GetSolver<MenuSolver<T>>().variables.Add(this);
        }

        internal List<Menu<T>> MenuInclusions = new List<Menu<T>>();
        internal List<Menu<T>> MenuExclusions = new List<Menu<T>>();
        internal List<T> Inclusions = new List<T>();
        internal List<T> Exclusions = new List<T>();

        /// <summary>
        /// When true, this variable takes its value from the specified menu
        /// </summary>
        public Proposition In(Menu<T> menu)
        {
            return Problem.GetSpecialProposition<MenuProposition<T>>(Call.FromArgs(Problem, "In", this, menu));
        }

        /// <summary>
        /// Default menu of values to use when no In assertion is true.
        /// </summary>
        public readonly Menu<T> BaseMenu;

        internal T CurrentValue;

        internal override object ValueInternal(Solution s)
        {
            return CurrentValue;
        }

        /// <inheritdoc />
        public override Domain<T> Domain => BaseMenu;

        /// <inheritdoc />
        public override T PredeterminedValue
        {
            get => throw new NotImplementedException();

            set => throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            // Do nothing, since there are no predetermined values
        }
    }
}
