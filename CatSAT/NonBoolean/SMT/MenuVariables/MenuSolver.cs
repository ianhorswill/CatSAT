#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuSolver.cs" company="Ian Horswill">
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
    class MenuSolver<T> : TheorySolver
    {
        /// <summary>
        /// All the MenuVariables used in this Problem.
        /// </summary>
        internal readonly List<MenuVariable<T>> Variables = new List<MenuVariable<T>>();
        /// <summary>
        /// All the SpecialPropositions, i.e. possible constraints, on MenuVariables inside this
        /// problem.
        /// </summary>
        internal readonly List<MenuProposition<T>> Propositions = new List<MenuProposition<T>>();

        public override bool Solve(Solution s)
        {
            // Compute new MenuInclusions lists for each variable
            
            foreach (var v in Variables)
                v.MenuInclusions.Clear();
            foreach (var p in Propositions)
            {
                if (!s[p])
                    // The proposition is false anyway, so who cares?
                    continue;

                var c = (Call)p.Name;
                switch (c.Name)
                {
                    case "In":
                        // Add the menu to the variable's MenuInclusions
                        var v = (MenuVariable<T>)c.Args[0];
                        var m = (Menu<T>) c.Args[1];
                        v.MenuInclusions.Add(m);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown MenuProposition type {c.Name}");
                }
            }

            // Assign values to variables
            foreach (var v in Variables)
            {
                if (v.IsDefinedInInternal(s) && !SelectValue(v))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Choose a specific value for v in the current solution
        /// </summary>
        private static bool SelectValue(MenuVariable<T> v)
        {
            if (v.BaseMenu != null)
                v.CurrentValue = v.BaseMenu.Selections.RandomElement();
            else
            {
                if (v.MenuInclusions.Count == 0)
                    return false;
                v.CurrentValue = v.MenuInclusions.RandomElement().Selections.RandomElement();
            }

            return true;
        }
    }
}
