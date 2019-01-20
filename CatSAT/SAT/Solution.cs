#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Solution.cs" company="Ian Horswill">
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
#define RANDOMIZE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CatSAT
{
    /// <summary>
    /// The output of a program; a model satisfying the clauses of the Problem.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Model) + "}")]
    public class Solution
    {
        /// <summary>
        /// The Program for which this is a solution.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// States of the different propositions of the Program, indexed by proposition number.
        /// </summary>
        internal readonly bool[] Propositions;

        internal Solution(Problem problem)
        {
            Problem = problem;
            Propositions = new bool[problem.SATVariables.Count];
        }

        /// <summary>
        /// A string listing the true propositions in the solution
        /// </summary>
        public string Model
        {
            // ReSharper disable once UnusedMember.Local
            get
            {
                var b = new StringBuilder();
                var firstOne = true;
                b.Append("{");
                for (int i = 1; i < Propositions.Length; i++)
                {
                    if (Propositions[i] && !Problem.SATVariables[i].Proposition.IsInternal)
                    {
                        if (firstOne)
                            firstOne = false;
                        else
                            b.Append(", ");
                        b.Append(Problem.SATVariables[i].Proposition);
                    }
                }

                foreach (var v in Problem.Variables())
                {
                    if (v.IsDefinedIn(this))
                    {
                        if (firstOne)
                            firstOne = false;
                        else
                            b.Append(", ");
                        b.Append(v.ValueString(this));
                    }
                }
                b.Append("}");
                return b.ToString();
            }
        }

        #region Checking truth values
        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool this[Literal l] => IsTrue(l);

        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        public bool this[Proposition p] => IsTrue(p);

        /// <summary>
        /// Test the truth of a literal (positive or negative) in the model.
        /// </summary>
        /// <param name="literal">Index of the literal (negative value = negative literal)</param>
        /// <returns>True if the literal is true in the model</returns>
        public bool IsTrue(short literal)
        {
            Debug.Assert(literal != 0, "0 is not a valid literal value!");
            if (literal > 0)
                return Propositions[literal];
            return !Propositions[-literal];
        }

        /// <summary>
        /// Test the truth of a proposition/positive literal
        /// </summary>
        /// <param name="index">Index of the proposition</param>
        /// <returns>True if the proposition is true in the model</returns>
        public bool IsTrue(ushort index)
        {
            return Propositions[index];
        }

        /// <summary>
        /// Test the truth of the specified proposition within the model
        /// </summary>
        public bool IsTrue(Proposition p)
        {
            return IsTrue(p.Index);
        }

        /// <summary>
        /// Test the truth of the specified literal within the model
        /// </summary>
        public bool IsTrue(Literal l)
        {
            switch (l)
            {
                case Proposition p:
                    return IsTrue(p);

                case Negation n:
                    return !IsTrue(n.Proposition);

                default:
                    throw new ArgumentException($"Internal error - invalid literal {l}");
            }
        }
        #endregion

        #region Quantifiers
        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function to map domain element to proposition</param>
        /// <typeparam name="T">Element type of domain</typeparam>
        /// <returns>True if the right number of elements are true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Quantify<T>(int min, int max, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Quantify(min, max, domain.Select(f));
        }

        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if the right number of elements are true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Quantify(int min, int max, params Literal[] literals)
        {
            return Quantify(min, max, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Test if the number of true literals is in the specified range
        /// </summary>
        /// <param name="min">Minimum number</param>
        /// <param name="max">Maximum number</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if the right number of elements are true in this solution</returns>
        public bool Quantify(int min, int max, IEnumerable<Literal> literals)
        {
            var enumerable = literals as Literal[] ?? literals.ToArray();
            if (max == 0)
            {
                max = enumerable.Length;
            }
            var c = Count(enumerable);
            return c >= min && c <= max;
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>Number of literals that are true</returns>
        public int Count(params Literal[] literals)
        {
            return Count((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <returns>Number of literals that are true</returns>
        // ReSharper disable once UnusedMember.Global
        public int Count<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Count(domain.Select(f));
        }

        /// <summary>
        /// Returns the number of literals from the specified set that are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>Number of literals that are true</returns>
        public int Count(IEnumerable<Literal> literals)
        {
            return literals.Count(IsTrue);
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if they're all true</returns>
        public bool All(params Literal[] literals)
        {
            return All((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <returns>True if they're all true</returns>
        public bool All<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return All(domain.Select(f));
        }

        /// <summary>
        /// Test if all the specified literals are true in this solution
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if they're all true</returns>
        public bool All(IEnumerable<Literal> literals)
        {
            var lits = literals.ToArray();
            return Quantify(lits.Length, lits.Length, (IEnumerable<Literal>)lits);
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists(params Literal[] literals)
        {
            return Exists((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Exists(domain.Select(f));
        }

        /// <summary>
        /// True if at least one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exists(IEnumerable<Literal> literals)
        {
            return literals.Any(IsTrue);
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly one literal is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Unique(params Literal[] literals)
        {
            return Unique((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <returns>True if exactly one literal is true in this solution</returns>
        public bool Unique<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Unique(domain.Select(f));
        }

        /// <summary>
        /// True if exactly one literal is true from the specified set
        /// </summary>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly one literal is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Unique(IEnumerable<Literal> literals)
        {
            return Quantify(1, 1, literals);
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exactly(int n, params Literal[] literals)
        {
            return Exactly(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Maps a domain element to a literal</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool Exactly<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return Exactly(n, domain.Select(f));
        }

        /// <summary>
        /// True if exactly the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if exactly the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Exactly(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, n, literals);
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtMost(int n, params Literal[] literals)
        {
            return AtMost(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Function to map a domain element to a literal</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtMost<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return AtMost(n, domain.Select(f));
        }

        /// <summary>
        /// True if at most the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at most the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool AtMost(int n, IEnumerable<Literal> literals)
        {
            return Quantify(0, n, literals);
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast(int n, params Literal[] literals)
        {
            return AtLeast(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="domain">Domain to quantify over</param>
        /// <param name="f">Function to map a domain element to a literal</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            return AtLeast(n, domain.Select(f));
        }

        /// <summary>
        /// True if at least the specified number of literals are true from the specified set
        /// </summary>
        /// <param name="n">Number of elements to test for</param>
        /// <param name="literals">Literals to test</param>
        /// <returns>True if at least the specified number is true in this solution</returns>
        // ReSharper disable once UnusedMember.Global
        public bool AtLeast(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, 0, literals);
        }
        #endregion

        #region Variables
        /// <summary>
        /// Get untyped value of variable
        /// </summary>
        public object this[Variable v] => v.UntypedValue(this);
        #endregion
    }
}
