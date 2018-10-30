#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuVariable.cs" company="Ian Horswill">
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

namespace CatSAT.NonBoolean.SMT.MenuVariables
{
    public class MenuVariable<T> : TheoryVariable<T>
    {
        public MenuVariable(object name, Menu<T> baseMenu, Problem problem, Literal condition = null) : base(name, problem, condition)
        {
            BaseMenu = baseMenu;
            problem.GetSolver<MenuSolver<T>>().variables.Add(this);
        }

        internal List<Menu<T>> MenuInclusions = new List<Menu<T>>();
        internal List<Menu<T>> MenuExclusions = new List<Menu<T>>();
        internal List<T> Inclusions = new List<T>();
        internal List<T> Exclusions = new List<T>();

        public Proposition In(Menu<T> menu)
        {
            return Problem.GetSpecialProposition<MenuProposition<T>>(Call.FromArgs(Problem, "In", this, menu));
        }

        public readonly Menu<T> BaseMenu;

        internal T CurrentValue;

        /// <inheritdoc />
        public override Domain<T> Domain => BaseMenu;

        /// <inheritdoc />
        public override T Value(Solution s)
        {
            return CurrentValue;
        }

        /// <inheritdoc />
        public override T PredeterminedValue()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void SetPredeterminedValue(T newValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
