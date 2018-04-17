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

namespace PicoSAT
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
        public abstract int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition);

        /// <summary>
        /// Make a conjunction of two Expressions.
        /// Performs simple constant folding, e.g. false & p = False, true & p = p.
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
    public abstract class Literal : Expression
#pragma warning restore 660,661
    {
        public static implicit operator Literal(string s)
        {
            return Proposition.MakeProposition(s);
        }

        public static Biconditional operator ==(Literal head, Expression body)
        {
            return new Biconditional(head, body);
        }

        public static Biconditional operator !=(Literal head, Expression body)
        {
            throw new NotImplementedException("!= is undefined for PicoSAT expressions");
        }

        /// <summary>
        /// The position of this literal in the Variables array of this literal's Problem.
        /// This is also the position of its truth value in the propositions array of a Solution to the Problem.
        /// IMPORTANT: if this is a negation, then the index is negative.  That is, if p has index 2, then
        /// Not(p) has index -2.
        /// </summary>
        public abstract short SignedIndex { get; }

        public override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
        {
            clauseArray[startingPosition] = (short)-SignedIndex;
            return startingPosition + 1;
        }

        public static Implication operator <=(Expression body, Literal head)
        {
            throw new NotImplementedException("Use head <= body for rules");
        }

        public static Implication operator >=(Expression body, Literal head)
        {
            return new Implication(head, body);
        }

        internal override void Assert(Problem p)
        {
            p.Assert(this);
        }

        public static implicit operator Literal(bool b)
        {
            return (Proposition) b;
        } 
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
        public readonly object Name;
        /// <summary>
        /// Position in the Problem's Variables[] array of the Variable that tracks the truth value of
        /// this Proposition, or zero, if this is one of the constants True or False.
        /// The actual truth value in a given Solution is stored at this index in the Solution's 
        /// propositions[] array.
        /// </summary>
        internal readonly ushort Index;
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

        internal Proposition(object name, ushort index)
        {
            Name = name;
            Index = index;
        }

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

        public override string ToString()
        {
            return Name.ToString();
        }

        public static implicit operator Proposition(string name)
        {
            return MakeProposition(name);
        }

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
        public override short SignedIndex
        {
            get
            {
                Debug.Assert(Index != 0, "SignedIndex called on a constant.");
                return (short) Index;
            }
        }

        public static implicit operator Proposition(bool b)
        {
            return b ? True : False;
        }

        public static explicit operator bool(Proposition p)
        {
            if (!p.IsConstant)
                throw new ArgumentException("Can't convert a non-constant proposition to a boolean.");
            return Equals(p, True);
        }

        public static Rule operator <=(Proposition head, Expression body)
        {
            return new Rule(head, body);
        }

        public static Rule operator >=(Proposition head, Expression body)
        {
            throw new NotImplementedException(">= is undefined for PicoSAT expressions");
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
    /// Represents a negated proposition.
    /// </summary>
    public class Negation : Literal
    {
        // It's annoying that I have to make this.  I wish I had S-expressions.
        public readonly Proposition Proposition;

        public Negation(Proposition proposition)
        {
            Proposition = proposition;
        }
    
        public static Literal Not(Proposition p)
        {
            if (p.IsConstant)
            {
                return (bool)p ? Proposition.False : Proposition.True;
            }
            return Problem.Current.Negation(p);
        }

        public override string ToString()
        {
            return $"!{Proposition}";
        }

        public override short SignedIndex => (short)-Proposition.Index;

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
        public readonly Literal Head;
        public readonly Expression Body;

        public Implication(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

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
        public readonly Proposition Head;
        public readonly Expression Body;

        public Rule(Proposition head, Expression body)
        {
            Head = head;
            Body = body;
        }

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
        public readonly Literal Head;
        public readonly Expression Body;

        public Biconditional(Literal head, Expression body)
        {
            Head = head;
            Body = body;
        }

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
    class Conjunction : Expression
    {
        /// <summary>
        /// LHS of the conjunction
        /// </summary>
        public readonly Expression Left;
        /// <summary>
        /// RHS of the conjunction
        /// </summary>
        public readonly Expression Right;

        public Conjunction(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return $"{Left} & {Right}";
        }

        public override int Size => Left.Size + Right.Size;

        public override int WriteNegatedSignedIndicesTo(short[] clauseArray, int startingPosition)
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
