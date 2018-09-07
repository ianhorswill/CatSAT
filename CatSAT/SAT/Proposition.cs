#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Proposition.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace CatSAT
{
    /// <summary>
    /// Represents an unnegated proposition, independent of its value in a given solution.
    /// </summary>
    public class Proposition : Literal
    {
        /// <summary>
        /// A fixed Proposition representing true.
        /// This will be recognized by various parts of the compiler and treated accordingly.
        /// </summary>
        public static readonly Proposition True = new Proposition(true, 0);
        /// <summary>
        /// A fixed proposition representing false.
        /// This will be recognized by various parts of the compiler and treated accordingly.
        /// </summary>
        public static readonly Proposition False = new Proposition(false, 0);

        /// <summary>
        /// Arbitrary object that functions as the name of this proposition.
        /// Distinct Propositions should have distinct Names.
        /// </summary>
        public object Name { get; internal set; }

        /// <summary>
        /// Probability with which this proposition will be true in the solver's starting guess.
        /// Should be a number in the range [0,1].
        /// </summary>
        public float InitialProbability = 0.5f;

        /// <summary>
        /// Position in the Problem's Variables[] array of the Variable that tracks the truth value of
        /// this Proposition, or zero, if this is one of the constants True or False.
        /// The actual truth value in a given Solution is stored at this index in the Solution's 
        /// propositions[] array.
        /// </summary>
        internal ushort Index;

        /// <summary>
        /// This is an internal, compiler-generated proposition.  So don't print it when we print a model.
        /// </summary>
        internal bool IsInternal;

        /// <summary>
        /// Bodies of any rules for which this proposition is the head.
        /// These get converted at solution time into clauses by Problem.CompileRuleBodies().
        /// </summary>
        internal List<Expression> RuleBodies;
        /// <summary>
        /// Propositions that appear as positive literals in this Proposition's Rules.
        /// A depends on B if B helps justify A.
        /// </summary>
        internal List<Proposition> PositiveDependencies;
        
        /// <summary>
        /// True if this proposition object is one of the constants true or false.
        /// This is different from propositions that are real parts of a Problem, but that
        /// have had their values predetermined by axioms, optimization, or the user explicitly
        /// setting their values.
        /// </summary>
        public bool IsConstant => Index == 0;

        /// <summary>
        /// True if this proposition's name is a call to the specfied functor name (e.g. predicate name, action name, etc.).
        /// </summary>
        /// <param name="functorName">Name to check for</param>
        // ReSharper disable once UnusedMember.Global
        public bool IsCall(string functorName)
        {
            return Name is Call c && c.Name == functorName;
        }

        /// <summary>
        /// For propositions whose names are Calls, returns the index'th argument of the call.
        /// </summary>
        /// <typeparam name="T">Expected type of the argument</typeparam>
        /// <param name="index">Argument index</param>
        public T Arg<T>(int index)
        {
            if (Name is Call c)
                return (T) c.Args[index];
            throw new ArgumentException($"Proposition {Name} is not a call to a predicate");
        }

        internal Proposition(object name, ushort index)
        {
            Name = name;
            Index = index;
        }

        /// <summary>
        /// Make a proposition.  Does nothing.
        /// This is here only to make it protected so that nobody tries to new their own proposition objects.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        protected Proposition() { }

        /// <summary>
        /// Return the n'th argument of the predicate for which this proposition is a ground instance.
        /// </summary>
        /// <param name="n">Argument number</param>
        /// <returns>Argument to the predicate</returns>
        public object Arg(int n)
        {
            if (!(Name is Call c))
                throw new InvalidOperationException($"The proposition {Name} does not contain any arguments");
            return c.Args[n];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// Coerces a string to a proposition.
        /// Returns the proposition in Problem.Current with the specified name
        /// </summary>
        /// <param name="name">Name to search for.</param>
        public static implicit operator Proposition(string name)
        {
            return MakeProposition(name);
        }

        /// <summary>
        /// Returns the proposition within Problem.Current with the specified name, creating one if necessary.
        /// </summary>
        /// <param name="name">Name for the proposition</param>
        public static Proposition MakeProposition(object name)
        {
            return Problem.Current.GetProposition(name);
        }

        /// <summary>
        /// Used when checking for circular definitions.
        /// State the proposition in a DFS of the graph formed by the Propositions and their PositiveDependencies.
        /// </summary>
        internal WalkState State;

        internal enum WalkState : byte
        {
            Unvisited = 0,
            Pending = 1,
            Complete = 2
        }

        /// <summary>
        /// Position of the Proposition in its Program's Variables[] array and its Solutions' propositions[] array.
        /// </summary>
        internal override short SignedIndex
        {
            get
            {
                Debug.Assert(Index != 0, "SignedIndex called on a constant.");
                return (short) Index;
            }
        }

        /// <summary>
        /// Coerces a boolean to a proposition
        /// </summary>
        /// <param name="b">Boolean to check</param>
        /// <returns>Proposition.True or Proposition.False</returns>
        public static implicit operator Proposition(bool b)
        {
            return b ? True : False;
        }

        /// <summary>
        /// Coerces a constant proposition (Proposition.True or Proposition.False) to a boolean
        /// </summary>
        /// <param name="p">Proposition</param>
        /// <returns>Proposition's (fixed) truth value</returns>
        /// <exception cref="ArgumentException">If the proposition isn't Proposition.True or Proposition.False</exception>
        public static explicit operator bool(Proposition p)
        {
            if (!p.IsConstant)
                throw new ArgumentException("Can't convert a non-constant proposition to a boolean.");
            return Equals(p, True);
        }

        /// <summary>
        /// Returns a rule representing that the body justifies the truth of the head
        /// </summary>
        /// <param name="head">Proposition that can be justified by the body</param>
        /// <param name="body">Literal or conjunction of literals that would justify the truth of head.</param>
        /// <returns></returns>
        public static Rule operator <=(Proposition head, Expression body)
        {
            return new Rule(head, body);
        }

        /// <summary>
        /// This syntax is not supported.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">This is not supported</exception>
        public static Rule operator >=(Proposition head, Expression body)
        {
            throw new NotImplementedException(">= is undefined for CatSAT expressions");
        }

        /// <summary>
        /// Add another rule to this proposition.
        /// </summary>
        /// <param name="e"></param>
        internal void AddRuleBody(Expression e)
        {
            RequireHaveSupport();
            RuleBodies.Add(e);
        }

        internal override IEnumerable<Proposition> PositiveLiterals
        {
            get { yield return this; }
        }
        
        /// <summary>
        /// Mark this proposition as depending on another proposition.
        /// </summary>
        public void AddDependency(Proposition d)
        {
            if (PositiveDependencies == null)
                PositiveDependencies = new List<Proposition>();
            PositiveDependencies.Add(d);
        }

        /// <summary>
        /// Force supported model semantics for this proposition.
        /// If no rules are provided for it, it will always be false.
        /// </summary>
        public void RequireHaveSupport()
        {
            if (RuleBodies == null)
                RuleBodies = new List<Expression>();
        }
    }
}