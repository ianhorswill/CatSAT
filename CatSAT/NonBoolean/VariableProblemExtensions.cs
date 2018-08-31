#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VariableProblemExtensions.cs" company="Ian Horswill">
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
using System.Linq;
using System.Reflection;

namespace CatSAT
{
    public static class VariableProblemExtensions
    {
        public static Variable Instantiate(this Problem p, object name, VariableType t, Literal condition = null)
        {
            return t.Instantiate(name, p, condition);
        }

        public static void AllDifferent<T>(this Problem p, IEnumerable<FDVariable<T>> vars)
        {
            var fdVariables = vars as FDVariable<T>[] ?? vars.ToArray();
            var d = (FDomain<T>)fdVariables.First().Domain;
            foreach (var v in fdVariables)
                if (v.Domain != d)
                    throw new ArgumentException($"Variables in AllDifferent() call must have identical domains");
            foreach (var value in d.Values)
                p.AtMost(1, fdVariables.Select(var => var == value));
        }

        /// <summary>
        /// Fill in  the fields of target with the values of the variables and propositions
        /// with the same name, from a new solution to the specified problem.
        /// </summary>
        /// <param name="p">Problem to solve to get values from</param>
        /// <param name="target">Object to fill in</param>
        /// <param name="bindingFlags">Binding flags for field lookup (defaults to public)</param>
        public static void Populate(this Problem p, object target, BindingFlags bindingFlags = BindingFlags.Public)
        {
            p.Solve().Populate(target, bindingFlags);
        }

        /// <summary>
        /// Fill in  the fields of target with the values of the variables and propositions
        /// with the same name, from solution.
        /// </summary>
        /// <param name="s">Solution to get values from</param>
        /// <param name="target">Object to fill in</param>
        /// <param name="bindingFlags">Binding flags for field lookup (defaults to public)</param>
        public static void Populate(this Solution s, object target, BindingFlags bindingFlags = BindingFlags.Public)
        {
            var targetType = target.GetType();
            var fields = targetType.GetFields(bindingFlags | BindingFlags.Instance);
            foreach (var f in fields)
            {
                var name = f.Name;
                var fieldType = f.FieldType;
                if (fieldType == typeof(bool))
                {
                    // it's a proposition
                    if (s.Problem.HasPropositionNamed(name))
                    f.SetValue(target, s[name]);
                } else
                {
                    var v = s.Problem.VariableNamed(name);
                    if ((object)v != null)
                    {
                        f.SetValue(target, v.IsDefinedIn(s)?v.UntypedValue(s):null);
                    }
                }
            }
        }
    }
}
