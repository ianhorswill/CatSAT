#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Expression.cs" company="Ian Horswill">
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
    /// Something that can be Assert'ed in a Problem.
    /// </summary>
    public abstract class Assertable
    {
        /// <summary>
        /// Add this assertion to the Problem
        /// </summary>
        internal abstract void Assert(Problem p);
    }

    /// <summary>
    /// A propositional expression
    /// </summary>
    public abstract class Expression : Assertable
    {
        /// <summary>
        /// Number of literals in the expression
        /// </summary>
        public virtual int Size => 1;

        /// <summary>
        /// Walk the expression tree and write the indicies of its literals into the specified array.
        /// </summary>
        /// <param name="clauseArray">Array of signed indices for the clause we're writing this to.</param>
        /// <param name="startingPosition">Position to start writing</param>
        /// <returns></returns>
        internal abstract int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition);

        /// <summary>
        /// Make a conjunction of two Expressions.
        /// Performs simple constant folding, e.g. false and p = False, true and p = p.
        /// </summary>
        public static Expression operator &(Expression left, Expression right)
        {
            if (left is Proposition l && l.IsConstant)
                    return (bool)l ? right : Proposition.False;
            if (right is Proposition r && r.IsConstant)
                return (bool)r ? left : Proposition.False;
            return new Conjunction(left, right);
        }

        /// <summary>
        /// Coerce an expression to a boolean
        /// Will throw an exception unless the expression is really a constant-valued Proposition.
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator Expression(bool b)
        {
            return (Proposition)b;
        }

        /// <summary>
        /// Find all the non-negated propositions in this Expression.
        /// </summary>
        internal abstract IEnumerable<Proposition> PositiveLiterals { get; }
    }

    /// <summary>
    /// Represents a Proposition or a Negation.
    /// </summary>
#pragma warning disable 660,661
    [DebuggerDisplay("{" + nameof(DebugName) + "}")]
    public abstract class Literal : Expression
#pragma warning restore 660,661
    {
        /// <summary>
        /// Coerce a string to a literal
        /// Returns the proposition with the specified name
        /// </summary>
        /// <param name="s">Name of hte proposition</param>
        /// <returns>The proposition with that naem</returns>
        public static implicit operator Literal(string s)
        {
            return Proposition.MakeProposition(s);
        }

        /// <summary>
        /// Returns a biconditional rule asserting that the head and body are true in exactly the same models
        /// </summary>
        /// <param name="head">Literal that's equivalent to the body</param>
        /// <param name="body">Literal or conjunction that's equivalent to the head.</param>
        /// <returns></returns>
        public static Biconditional operator ==(Literal head, Expression body)
        {
            return new Biconditional(head, body);
        }

        /// <summary>
        /// This is not actually supported
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Biconditional operator !=(Literal head, Expression body)
        {
            throw new NotImplementedException("!= is undefined for CatSAT expressions");
        }

        /// <summary>
        /// The position of this literal in the Variables array of this literal's Problem.
        /// This is also the position of its truth value in the propositions array of a Solution to the Problem.
        /// IMPORTANT: if this is a negation, then the index is negative.  That is, if p has index 2, then
        /// Not(p) has index -2.
        /// </summary>
        internal abstract short SignedIndex { get; }

        internal override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
        {
            clauseArray[startingPosition] = (short)-SignedIndex;
            return startingPosition + 1;
        }

        /// <summary>
        /// This syntax is not supported for implications or rules.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Implication operator <(Expression body, Literal head)
        {
            throw new NotImplementedException("Use head <= body for rules");
        }

        /// <summary>
        /// Returns an implication asserting that the body implied the head, but not vice-versa
        /// </summary>
        /// <param name="body">A literal or conjunction of literals</param>
        /// <param name="head">A literal implied by the body.</param>
        /// <returns></returns>
        public static Implication operator >(Expression body, Literal head)
        {
            return new Implication(head, body);
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }

        /// <summary>
        /// Coerces a boolean to a Proposition with a fixed truth value
        /// </summary>
        /// <param name="b">Boolean</param>
        /// <returns>Proposition - either Proposition.True or Proposition.False</returns>
        public static implicit operator Literal(bool b)
        {
            return (Proposition) b;
        }

        private string DebugName => ToString();
    }

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
        /// </summary>
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

    /// <summary>
    /// A proposition that involves specialized processing.
    /// </summary>
    public class SpecialProposition : Proposition
    {
        /// <summary>
        /// Called automatically after constructor to initialize the special problem instance.
        /// </summary>
        /// <param name="p">Problem to which this proposition belongs</param>
        public virtual void Initialize(Problem p) { }
    }

    /// <summary>
    /// Represents a negated proposition.
    /// </summary>
    public class Negation : Literal
    {
        /// <summary>
        /// The proposition being negated
        /// </summary>
        public readonly Proposition Proposition;

        /// <summary>
        /// Creates a Literal representing the negation of the proposition
        /// </summary>
        /// <param name="proposition">Proposition to negate</param>
        public Negation(Proposition proposition)
        {
            Proposition = proposition;
        }
    
        /// <summary>
        /// Creates a Literal representing the negation of the proposition
        /// </summary>
        /// <param name="p">Proposition to negate</param>
        public static Literal Not(Proposition p)
        {
            if (p.IsConstant)
            {
                return (bool)p ? Proposition.False : Proposition.True;
            }
            return Problem.Current.Negation(p);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"!{Proposition}";
        }

        internal override short SignedIndex => (short)-Proposition.Index;

        internal override IEnumerable<Proposition> PositiveLiterals
        {
            get
            {
                // This is a negative literal, not a positive one.
                yield break;
            }
        }
    }

    /// <summary>
    /// States that the Body being true forces the Head to be true without placing any constraint on Body.
    /// </summary>
    public class Implication : Assertable
    {
        /// <summary>
        /// The literal implied by the assertion
        /// </summary>
        public readonly Literal Head;
        /// <summary>
        /// The condition under which the head is implied
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// Creates an expression representing that a given Literal or conjunction of literals implies the specifed literal.
        /// </summary>
        /// <param name="head">literal implied by the body</param>
        /// <param name="body">literal or conjunction that implies the head</param>
        public Implication(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Head} <= {Body}";
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }
    }

    /// <summary>
    /// States that Body is a possible justification for the truth of Head.
    /// This is different from an implication.  An implication says that Body being true forces Head to be true
    /// and nothing more.  A rule gives the Head completion semantics.  It says that the Body being true forces 
    /// the Head to be true, but also says that the Head being true forces at least one of its rule bodies to be
    /// true.
    /// </summary>
    public class Rule : Assertable
    {
        /// <summary>
        /// The proposition being concluded by the rule
        /// </summary>
        public readonly Proposition Head;
        /// <summary>
        /// The literal or conjunction that would justify concluding the head.
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// A rule that states that the head is justified by the body
        /// </summary>
        /// <param name="head">proposition that can be concluded from the body</param>
        /// <param name="body">Literal or conjunction that would justify the head</param>
        public Rule(Proposition head, Expression body)
        {
            Head = head;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Head} <= {Body}";
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }
    }

    /// <summary>
    /// Constrains Head and Body to have the same truth value.
    /// </summary>
    public class Biconditional : Assertable
    {
        /// <summary>
        /// Literal stated to be equivalent to the body
        /// </summary>
        public readonly Literal Head;
        /// <summary>
        /// Literal or conjunction stated to be equivalent to the head
        /// </summary>
        public readonly Expression Body;

        /// <summary>
        /// An expression stating that the head and body are equivalent (true in the same models)
        /// </summary>
        /// <param name="head">literal equivalent to the body</param>
        /// <param name="body">Literal or conjunction equivalent to the head</param>
        public Biconditional(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Head} == {Body}";
        }
        
        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }
    }

    /// <summary>
    /// A conjunction of literals.
    /// Used in rule bodies.
    /// </summary>
    public class Conjunction : Expression
    {
        /// <summary>
        /// LHS of the conjunction
        /// </summary>
        public readonly Expression Left;
        /// <summary>
        /// RHS of the conjunction
        /// </summary>
        public readonly Expression Right;

        /// <summary>
        /// An expression representing the condition in which both left and right are true
        /// </summary>
        /// <param name="left">LHS of the conjunction</param>
        /// <param name="right">RHS of the conjunction</param>
        public Conjunction(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Left} & {Right}";
        }

        /// <summary>
        /// Number of terms in the conjunction
        /// </summary>
        public override int Size => Left.Size + Right.Size;

        internal override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
        {
            return Right.WriteNegatedSignedIndicesTo(clauseArray, Left.WriteNegatedSignedIndicesTo(clauseArray, startingPosition));
        }
        
        internal override void Assert(Problem p)
        {
            Left.Assert(p);
            Right.Assert(p);
        }

        internal override IEnumerable<Proposition> PositiveLiterals
        {
            get
            {
                foreach (var p in Left.PositiveLiterals)
                    yield return p;

                foreach (var p in Right.PositiveLiterals)
                    yield return p;
            }
        }
    }
}
