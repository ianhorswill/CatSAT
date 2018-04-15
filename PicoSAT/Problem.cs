#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Problem.cs" company="Ian Horswill">
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
using System.Linq;
using System.Text;
using static PicoSAT.Language;

namespace PicoSAT
{
    /// <summary>
    /// A logic program.
    /// Contains a set of prositions, rules for the propositions, and general clauses.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class Problem
    {
        //
        // This is basically a storage area for Propositions, Variables, and Clauses (constraints)
        // It maintains bookkeeping information aobut how they all relate to one another and it
        // compiles rules to their clauses.  But otherwise, it's mostly passive.  It doesn't contain
        // the actual SAT solver, which is in the Solution object
        //

        /// <summary>
        /// Current problem into which to intern propositions.
        /// This is needed because when you create a proposition or negation, it needs to be placed in a
        /// hash table so that when you look it up in the future, you get the same proposition/literal.
        /// That means we need to know which Problem it's a part of, so we can add it to the right table.
        /// </summary>
        public static Problem Current;

        /// <summary>
        /// Make a new logic program into which we can enter propositions and rules.
        /// </summary>
        /// <param name="name"></param>
        public Problem(string name = "unnamed")
        {
            Name = name;
            Current = this;
            Variables.Add(new Variable(new Proposition("I am not a valid proposition!  I am a placeholder!", 0)));
        }

        #region Instance variables
        enum CompilationState : byte
        {
            Uncompiled = 0,
            HaveRules = 1,
            Compiled = 2
        }

        /// <summary>
        /// Tracks whether we've already compiled the rules for this program.
        /// </summary>
        private CompilationState compilationState = CompilationState.Uncompiled;

        private string DebuggerDisplay
        {
            // ReSharper disable once UnusedMember.Local
            get
            {
                var b = new StringBuilder();
                var firstClause = true;
                foreach (var c in Clauses)
                {
                    if (firstClause)
                        firstClause = false;
                    else
                        b.Append(" & ");

                    var firstLit = true;
                    foreach (var d in c.Disjuncts)
                    {
                        if (firstLit)
                            firstLit = false;
                        else
                            b.Append(" | ");
                        if (d < 0)
                            b.Append("!");
                        b.Append(Variables[Math.Abs(d)].Proposition);
                    }

                }

                return b.ToString();
            }
        }

        /// <summary>
        /// Name of the Problem, for debugging purposes.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Number of flips of propositions we can try before we give up and start over.
        /// </summary>
        public int MaxFlips = 1000;

        /// <summary>
        /// Number of times we can start over before we give up entirely.
        /// </summary>
        public int MaxTries = 100;

        /// <summary>
        /// Probability of just doing a random flip rather than specifically one from an unsatisfied clause
        /// </summary>
        public int RandomFlipProbability = 10;

        /// <summary>
        /// Require the program to be tight, i.e. not allow circular reasoning chains.
        /// </summary>
        public bool Tight = true;

        /// <summary>
        /// Number of timesteps in history, if fluents are used.
        /// </summary>
        public int TimeHorizon=-1;
        
        /// <summary>
        /// The Variables in the Problem.
        /// There is one Variable for each Proposition.  And the Solution assigns a truth value to that 
        /// variable.
        /// </summary>
        internal readonly List<Variable> Variables = new List<Variable>();
        /// <summary>
        /// The constraints in the Problem.
        /// Most of these are normal clauses (disjunctions), but other cardinality constraints are possible.
        /// </summary>
        internal readonly List<Clause> Clauses = new List<Clause>();

        /// <summary>
        /// All the Propositions used in the Problem.
        /// </summary>
        public IEnumerable<Proposition> Propositions
        {
            get
            {
                foreach (var pair in propositionTable)
                    yield return pair.Value;
            }
        }
        #endregion

        #region Clause management
        /// <summary>
        /// Forcibly add a clause to the Problem.
        /// </summary>
        internal Clause AddClause(params Literal[] disjuncts)
        {
            return AddClause(1, 0, disjuncts);
        }

        /// <summary>
        /// Forcibly add a clause to the Problem.
        /// </summary>
        internal Clause AddClause(ushort min, ushort max, params Literal[] disjuncts)
        {
            // Look up the internal numeric literal representations for all the disjuncts
            var compiled = CompileClause(disjuncts);
            var clause = new Clause(min, max, compiled);
            AddClause(clause);

            return clause;
        }

        /// <summary>
        /// Forcibly add a clause to the Problem.
        /// </summary>
        private void AddClause(Clause clause)
        {
            Clauses.Add(clause);

            // Add the clause to the appropriate clause list for all the propositions that appear in the clause
            var clauseIndex = (ushort) (Clauses.Count - 1);
            foreach (var lit in clause.Disjuncts)
            {
                if (lit > 0)
                    Variables[lit].PositiveClauses.Add(clauseIndex);
                else
                    Variables[-lit].NegativeClauses.Add(clauseIndex);
            }
        }

        /// <summary>
        /// Map an array of Literals to an array of their signed indices.
        /// </summary>
        private short[] CompileClause(Literal[] disjuncts)
        {
            var indices = new short[disjuncts.Length];
            for (var i = 0; i < indices.Length; i++)
            {
                var expression = disjuncts[i];
                indices[i] = expression.SignedIndex;
            }

            return indices;
        }
        #endregion

        /// <summary>
        /// Return a Solution to the Problem.
        /// If not solution can be found, either return null or throw UnsatisfiableException.
        /// </summary>
        /// <param name="throwOnUnsolvable">If true, will throw UnsatisfiableException when no solution is found.</param>
        /// <returns>The Solution object mapping propositions to truth values.</returns>
        public Solution Solve(bool throwOnUnsolvable = true)
        {
            FinishCodeGeneration();
            var m = new Solution(this, MaxFlips, MaxTries, RandomFlipProbability);
            if (m.Solve())
                return m;
            if (throwOnUnsolvable)
                throw new UnsatisfiableException(this);
            return null;
        }

        #region Assertions

        public void Assert(params Assertable[] assertions)
        {
            foreach (var a in assertions)
                a.Assert(this);
        }

        public void Assert(Literal l)
        {
            if (Equals(l, Proposition.True))
                // We already know that true is true.
                return;
            if (Equals(l, Proposition.False))
                throw new InvalidOperationException("Attempt to Assert the false proposition.");
            //AddClause(new Clause(1, 0, new[] {l.SignedIndex}));
            switch (l)
            {
                case Proposition p:
                    MakeConstant(p, true);
                    break;

                case Negation n:
                    MakeConstant(n.Proposition, false);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown literal type: {l.GetType().Name}");
            }
        }

        private void MakeConstant(Proposition p, bool value)
        {
            MakeConstant(p.Index, value);
        }

        private void MakeConstant(int index, bool value)
        {
            var v = Variables[index];
            v.SetConstant(value);
            Variables[index] = v;
        }

        public void Assert(Implication i)
        {
            var h = i.Head;
            if (h is Proposition p && p.IsConstant)
            {
                if ((bool)p)
                    // true <= X is always true
                    return;
                throw new InvalidOperationException("Consequent of implication cannot be the constant False.");
            }
            AddClause(CompileImplication(i));
        }

        public void Assert(Rule r)
        {
            if (compilationState == CompilationState.Compiled)
                throw new InvalidOperationException("Can't add rules after calling Solve().");

            if (r.Head.IsConstant)
            {
                throw new InvalidOperationException("Rule heads cannot be constants.");
            }

            foreach (var d in r.Body.PositiveLiterals)
                if (!d.IsConstant)
                    r.Head.AddDependency(d);
            
            r.Head.AddRuleBody(r.Body);
            compilationState = CompilationState.HaveRules;
        }

        public void Assert(Biconditional b)
        {
            // Compile the forward implication
            var disjuncts = DisjunctsFromImplication(b.Head, b.Body);
            AddClause(new Clause(1, 0, disjuncts));

            // Now compile the backward implication: if the head is true, all the body literals have to be true.
            for (var i = 1; i < disjuncts.Length; i++)
            {
                AddClause(new Clause(1, 0, new [] { (short)-disjuncts[0], (short)-disjuncts[i]}));
            }
        }

        private Clause CompileImplication(Implication implication)
        {
            return new Clause(1, 0, DisjunctsFromImplication(implication.Head, implication.Body));
        }

        private short[] DisjunctsFromImplication(Literal head, Expression body)
        {
            var result = new short[body.Size + 1];
            result[0] = head.SignedIndex;
            body.WriteNegatedSignedIndicesTo(result, 1);

            return result;
        }

        private void FinishCodeGeneration()
        {
            if (compilationState == CompilationState.HaveRules)
            {
                if (Tight)
                    CheckTightness();
                CompileRuleBodies();
            }

            compilationState = CompilationState.Compiled;
        }

        private void CheckTightness()
        {
            void Walk(Proposition p)
            {
                switch (p.State)
                {
                    case Proposition.WalkState.Complete:
                        return;

                    case Proposition.WalkState.Pending:
                        throw new NonTightProblemException(p);

                    case Proposition.WalkState.Unvisited:
                        p.State = Proposition.WalkState.Pending;
                        if (p.PositiveDependencies != null)
                            foreach (var d in p.PositiveDependencies)
                                Walk(d);
                        p.State = Proposition.WalkState.Complete;
                        return;
                }
            }

            for (int i = 0; i < Variables.Count; i++)
                Walk(Variables[i].Proposition);
        }

        private void CompileRuleBodies()
        {
            int startingVariableCount = Variables.Count;

            for (int i = 0; i < startingVariableCount; i++)
            {
                var v = Variables[i];
                var p = v.Proposition;
                var bodies = p.RuleBodies;
                if (bodies != null)
                {
                    Debug.Assert(bodies.Count > 0);
                    AssertCompletion(v.Proposition, bodies);
                }
            }

#if !DEBUG
                CleanPropositionInfo();
#endif
        }

#if !DEBUG
        private void CleanPropositionInfo()
        {
            foreach (var v in Variables)
            {
                var p = v.Proposition;
                p.RuleBodies = null;
                p.PositiveDependencies = null;
            }
        }
#endif

        private void AssertCompletion(Proposition p, List<Expression> bodies)
        {
            if (p.IsConstant)
                throw new InvalidOperationException(
                    "True and False are not valid rule heads for completion semantics.");

            var trueBodies = 0;
            var falseBodies = 0;
            foreach (var b in bodies)
            {
                if (Equals(b, Proposition.True))
                    trueBodies++;
                else if (Equals(b, Proposition.False))
                    falseBodies++;
            }

            if (trueBodies > 0)
            {
                // p is always true
                Assert(p);
            }
            else if (bodies.Count == falseBodies)
            {
                // The only bodies are false, so p is always false
                Assert(Not(p));
            }
            else if (bodies.Count == 1)
                Assert(new Biconditional(p, bodies[0]));
            else
            {
                if (falseBodies > 0)
                    bodies.RemoveAll(b => Equals(b, Proposition.False));
                var justifications = new short[bodies.Count];
                var compiledBodies = new short[bodies.Count][];

                for (int i = 0; i < bodies.Count; i++)
                {
                    var body = bodies[i];

                    // Compile the forward implication
                    var disjuncts = DisjunctsFromImplication(p, body);
                    compiledBodies[i] = disjuncts;

                    AddClause(new Clause(1, 0, disjuncts));

                    if (disjuncts.Length == 2)
                        justifications[i] = (short) -disjuncts[1];
                    else
                    {
                        var justificationProp = GetProposition(body).SignedIndex;
                        justifications[i] = justificationProp;

                        // Now compile the backward implication: if the head is true, all the body literals have to be true.
                        for (var j = 1; j < disjuncts.Length; j++)
                        {
                            // justificationProp => disjuncts[j]
                            AddClause(new Clause(1, 0,
                                new[] {(short) -justificationProp, (short) -disjuncts[j]}));
                        }
                    }
                }

                var reverseClause = new short[bodies.Count + 1];
                reverseClause[0] = (short) -p.SignedIndex;

                Array.Copy(justifications, 0, reverseClause, 1, justifications.Length);

                AddClause(new Clause(1, 0, reverseClause));
            }
        }
        #endregion

        #region Quantifiers
        public void Quantify(int min, int max, IEnumerable<Literal> enumerator)
        {
            Quantify(min, max, enumerator.Select(l => l.SignedIndex).ToArray());
        }

        public void Quantify(int min, int max, short[] disjuncts)
        {
            AddClause(new Clause((ushort)min, (ushort)max, disjuncts));
        }

        public void All(IEnumerable<Literal> enumerator)
        {
            var disjuncts = enumerator.Select(l => l.SignedIndex).ToArray();
            Quantify(disjuncts.Length, disjuncts.Length, disjuncts);
        }

        public void Exists(IEnumerable<Literal> enumerator)
        {
            Quantify(1, 0, enumerator);
        }

        public void Unique(IEnumerable<Literal> enumerator)
        {
            Quantify(1, 1, enumerator);
        }

        public void Exactly(int n, IEnumerable<Literal> enumerator)
        {
            Quantify(n, n, enumerator);
        }

        public void AtMost(int n, IEnumerable<Literal> enumerator)
        {
            Quantify(0, n, enumerator);
        }

        public void AtLeast(int n, IEnumerable<Literal> enumerator)
        {
            Quantify(n, 0, enumerator);
        }
        #endregion

        #region Mapping between Literals objects and Variables
        private readonly Dictionary<object, Proposition> propositionTable = new Dictionary<object, Proposition>();

        public Proposition GetProposition(object key)
        {
            // It's a constant
            if (key is bool b)
                return b ? Proposition.True : Proposition.False;

            // It's already in the table
            if (propositionTable.TryGetValue(key, out Proposition p))
                return p;

            // It's a new proposition
            p = new Proposition(key, (ushort) Variables.Count);
            Variables.Add(new Variable(p));
            propositionTable[key] = p;
            return p;
        }

        private readonly Dictionary<Proposition, Negation> negationTable = new Dictionary<Proposition, Negation>();

        public Negation Negation(Proposition key)
        {
            if (negationTable.TryGetValue(key, out Negation p))
                return p;
            p = new Negation(key);
            negationTable[key] = p;
            return p;
        }

        internal Proposition KeyOf(Clause clause, ushort position)
        {
            return Variables[clause.Disjuncts[position]].Proposition;
        }

        /// <summary>
        /// True if proposition is known to be true in all models
        /// </summary>
        public bool IsAlwaysTrue(Proposition p)
        {
            return Variables[p.Index].IsAlwaysTrue;
        }

        /// <summary>
        /// True if proposition is known to be false in all models
        /// </summary>
        public bool IsAlwaysFalse(Proposition p)
        {
            return Variables[p.Index].IsAlwaysFalse;
        }

        /// <summary>
        /// True if proposition is known to have the same value in all models
        /// </summary>
        public bool IsConstant(Proposition p)
        {
            return Variables[p.Index].IsConstant;
        }
        #endregion

        #region Optimization (unit resolution)
        /// <summary>
        /// Do a simple constant-folding pass over the program.
        /// This is technically called unit resolution, but it basically means constant folding
        /// </summary>
        public void Optimize()
        {
            FinishCodeGeneration();

            // This is inefficient, but I'm not going to optimize this loop until I know it's worthwhile
            bool changed;
            var toBeOptimized = new List<Clause>(Clauses);
            do
            {
                changed = false;
                foreach (var c in toBeOptimized.ToArray())
                    switch (UnitPropagate(c))
                    {
                        case OptimizationState.Contradiction:
                            // Compile-time false
                            throw new UnsatisfiableException(this);

                        case OptimizationState.Optimized:
                            // Just optimized it away
                            changed = true;
                            toBeOptimized.Remove(c);
                            break;

                        case OptimizationState.Ignore:
                            // Compile-time true; don't need to look at it any more
                            toBeOptimized.Remove(c);
                            break;
                    }
            } while (changed);
        }

        enum OptimizationState { InPlay, Ignore, Optimized, Contradiction }

        OptimizationState UnitPropagate(Clause c)
        {
            if (!c.IsNormalDisjunction)
                // Unit resolution is invalid for this clause
                return OptimizationState.Ignore;

            // Check if clause has exactly one disjunct that isn't verifiably false at compile time
            short inferred = 0;
            foreach (var i in c.Disjuncts)
            {
                if (i > 0)
                {
                    // It's a positive literal
                    var v = Variables[i];
                    if (v.IsConstant)
                    {
                        if (v.ConstantValue)
                            // It's a positive literal and it's true, so no optimization possible
                            return OptimizationState.Ignore;
                        // Otherwise, v is always false, so this disjunct is always false
                        // Move on to the next disjunct
                    }
                    else
                    {
                        if (inferred != 0)
                            // We already found a non-constant disjunct, so this is the second one
                            // and so we can't do anything with this clause
                            return OptimizationState.InPlay;
                        inferred = i;
                    }
                }
                else
                {
                    // It's a negative literal
                    var v = Variables[-i];
                    if (v.IsConstant)
                    {
                        if (!v.ConstantValue)
                            // It's a negative literal and it's false so no optimization possible
                            return OptimizationState.Ignore;
                        // Otherwise, v is always false, so this disjunct is always false
                        // Move on to the next disjunct
                    }
                    else
                    {
                        if (inferred != 0)
                            // We already found a non-constant disjunct, so this is the second one
                            // and so we can't do anything with this clause
                            return OptimizationState.InPlay;
                        inferred = i;
                    }
                }
            }

            // We got through all the disjuncts
            if (inferred == 0)
                // All disjuncts are compile-time false!
                return OptimizationState.Contradiction;
            if (inferred > 0)
                MakeConstant(inferred, true);
            else
                MakeConstant(-inferred, false);
            return OptimizationState.Optimized;
        }

        #endregion
    }
}
