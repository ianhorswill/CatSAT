#define NewOptimizer
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
#if PerformanceStatistics
            Stopwatch.Reset();
            Stopwatch.Start();
#endif
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

#if PerformanceStatistics
        internal readonly Stopwatch Stopwatch = new Stopwatch();

        /// <summary>
        /// Number of microseconds spent in Boolean Constriant Propagation
        /// </summary>
        public float OptimizationTime { get; private set; }
        /// <summary>
        /// Number of microsections spent computing the completions of rules
        /// </summary>
        public float CompilationTime { get; private set; }
        /// <summary>
        /// Number of microseconds spent building the problem object itself.
        /// This is the time spent making propositions and predicates, calling Assert, etc.
        /// It's measured as elapsed time from the calling of the constructor to the calling
        /// of the FinishRuleCompilation() method.
        /// </summary>
        public float CreationTime { get; private set; }

        public struct TimingData
        {
            public float Min;
            public float Max;
            private float sum;
            private int count;

            public float Average => count==0?0:sum / count;

            public void AddReading(float reading)
            {
                if (count == 0)
                {
                    Min = float.MaxValue;
                    Max = float.MinValue;
                }
                if (reading < Min)
                    Min = reading;
                if (reading > Max)
                    Max = reading;
                sum += reading;
                count++;
            }
        }

        // ReSharper disable once UnassignedField.Global
        public TimingData SolveTimeMicroseconds;
        // ReSharper disable once UnassignedField.Global
        public TimingData SolveFlips;


        /// <summary>
        /// Always print performance data to the console.
        /// </summary>
        public static bool LogPerformanceDataToConsole;

        private static string _logFile;
        public static string LogFile
        {
            get => _logFile;
            set
            {
                _logFile = value;
                System.IO.File.WriteAllLines(_logFile, new [] {"Name,Create,Compile,Optimize,Clauses,Variables,Floating,SolveMin, SolveMax,SolveAvg,FlipsMin,FlipsMax,FlipsAvg"});
            }
        }

        public void LogPerformanceData()
        {
            System.IO.File.AppendAllLines(LogFile, new []
            {
                $"'{Name}',{CreationTime},{CompilationTime},{OptimizationTime},{Clauses.Count},{Variables.Count},{Variables.Count(v => v.DeterminionState == Variable.DeterminationState.Floating)},{SolveTimeMicroseconds.Min},{SolveTimeMicroseconds.Max},{SolveTimeMicroseconds.Average},{SolveFlips.Min},{SolveFlips.Max},{SolveFlips.Average}"
            });
        }

        ~Problem()
        {
            if (_logFile != null) LogPerformanceData();
        }
#endif

        public string PerformanceStatistics
        {
            get
            {
#if PerformanceStatistics
                return $"Creation: {CreationTime:#,##0}us, Compilation: {CompilationTime:#,##0.##}us, Optimization: {OptimizationTime:#,##0.##}us, C+O: {CompilationTime+OptimizationTime:0}us";
#else
                return "PicoSAT compiled without performance measurements";
#endif
            }
        }

        public string Stats
        {
            get
            {
                return
                    $"{Variables.Count} variables, {Variables.Count(v => v.DeterminionState == Variable.DeterminationState.Floating)} floating, {Clauses.Count} clauses";
            }
        }
        
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
        // ReSharper disable once NotAccessedField.Global
        public readonly string Name;

        /// <summary>
        /// Number of flips of propositions we can try before we give up and start over.
        /// </summary>
        public int Timeout = 50000;

        /// <summary>
        /// Require the program to be tight, i.e. not allow circular reasoning chains.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
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

        internal readonly List<ushort> FloatingVariables = new List<ushort>();

        /// <summary>
        /// All the Propositions used in the Problem.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
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
            clause.Index = (ushort)Clauses.Count;
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
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public Solution Solve(bool throwOnUnsolvable = true)
        {
#if PerformanceStatistics
            var previousState = compilationState;
#endif
            FinishCodeGeneration();
#if PerformanceStatistics
            if (LogPerformanceDataToConsole && previousState != CompilationState.Compiled)
                Console.WriteLine(PerformanceStatistics);
#endif
            if (FloatingVariables.Count == 0)
                RecomputeFloatingVariables();
            var m = new Solution(this, Timeout);
            if (m.Solve())
            {
#if PerformanceStatistics
                SolveTimeMicroseconds.AddReading(m.SolveTimeMicroseconds);
                SolveFlips.AddReading(m.SolveFlips);
                if (LogPerformanceDataToConsole)
                    Console.WriteLine(m.PerformanceStatistics);
#endif
                return m;
            }
            if (throwOnUnsolvable)
                throw new TimeoutException(this);
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
                    SetPredeterminedValue(p, true, Variable.DeterminationState.Fixed);
                    break;

                case Negation n:
                    SetPredeterminedValue(n.Proposition, false, Variable.DeterminationState.Fixed);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown literal type: {l.GetType().Name}");
            }
        }

        private void SetPredeterminedValue(Proposition p, bool value, Variable.DeterminationState s)
        {
            SetPredeterminedValue(p.Index, value, s);
        }

        private void SetPredeterminedValue(int index, bool value, Variable.DeterminationState s)
        {
            var v = Variables[index];
            v.DeterminionState = s;
            v.PredeterminedValue = value;
            Variables[index] = v;
        }

        public void Assert(Implication i)
        {
            var h = i.Head;
            if (h is Proposition p && p.IsConstant)
            {
                if (!(bool)p)
                    AddClause(CompileNegatedConjunction(i.Body));
                // Otherwise h is always true, so don't bother adding a clause
                return;
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

        private Clause CompileNegatedConjunction(Expression body)
        {
            return new Clause(1, 0, DisjunctsFromBody(body));
        }

        private short[] DisjunctsFromBody(Expression body)
        {
            var result = new short[body.Size];
            body.WriteNegatedSignedIndicesTo(result, 0);

            return result;
        }

        private void FinishCodeGeneration()
        {
            if (compilationState == CompilationState.HaveRules)
            {
#if PerformanceStatistics
                CreationTime = Stopwatch.ElapsedTicks / (0.000001f * Stopwatch.Frequency);
                Stopwatch.Reset();
                Stopwatch.Start();
#endif
                if (Tight)
                    CheckTightness();
                CompileRuleBodies();
#if PerformanceStatistics
                CompilationTime = Stopwatch.ElapsedTicks / (0.000001f * Stopwatch.Frequency);
#endif
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
                    AssertCompletion(v.Proposition, bodies);
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
                        var justificationProp = GetInternalProposition(body).SignedIndex;
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
            Quantify(min, max, enumerator.Select(l => l.SignedIndex).Distinct().ToArray());
        }

        public void Quantify(int min, int max, short[] disjuncts)
        {
            AddClause(new Clause((ushort)min, (ushort)max, disjuncts));
        }

        // ReSharper disable once UnusedMember.Global
        public void All(IEnumerable<Literal> enumerator)
        {
            var disjuncts = enumerator.Select(l => l.SignedIndex).ToArray();
            Quantify(disjuncts.Length, disjuncts.Length, disjuncts);
        }

        // ReSharper disable once UnusedMember.Global
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

        // ReSharper disable once UnusedMember.Global
        public void AtLeast(int n, IEnumerable<Literal> enumerator)
        {
            Quantify(n, 0, enumerator);
        }
#endregion

#region Mapping between Literals objects and Variables
        private readonly Dictionary<object, Proposition> propositionTable = new Dictionary<object, Proposition>();

        /// <summary>
        /// Get a proposition within this Problem, with the specified key
        /// </summary>
        /// <param name="key">Arbitrary object that acts as a name for this proposition.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a proposition and marks it internal
        /// </summary>
        internal Proposition GetInternalProposition(object key)
        {
            var p = GetProposition(key);
            p.IsInternal = true;
            return p;
        }

        public T GetPropositionOfType<T>(object key) where T: Proposition, new()
        {
            // It's already in the table
            if (propositionTable.TryGetValue(key, out Proposition old))
                return (T)old;

            // It's a new proposition
            var p = new T() { Name = key, Index = (ushort)Variables.Count };
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
        // ReSharper disable once UnusedMember.Global
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
            return Variables[p.Index].IsPredetermined;
        }
#endregion

#region Optimization (unit resolution)
        
#if NewOptimizer
        public void Optimize()
        {
            FinishCodeGeneration();
#if PerformanceStatistics
            Stopwatch.Reset();
            Stopwatch.Start();
#endif
            ResetInferredPropositions();

            // The number of literals in clause whose values aren't yet known.
            // Or -1 if this clause now compile-time true.
            var counts = new short[Clauses.Count];

            short UndeterminedDisjunctCount(Clause c)
            {
                if (counts[c.Index] != 0)
                    return counts[c.Index];
                if (!c.IsNormalDisjunction)
                    // Ignore this clause
                    return counts[c.Index] = -1;
                var count = CountUndeterminedDisjuncts(c);
                counts[c.Index] = count;
                return count;
            }

            var walkQueue = new Queue<Clause>();

            void Walk(Clause c)
            {
                var d = UndeterminedDisjunctCount(c);
                if (d == 1)
                {
                    // All the disjuncts but one are known to be false, so the last must be true.
                    var v = UndeterminedDisjunctOf(c);
                    if (v > 0)
                    {
                        // Positive literal, so it must be true
                        SetPredeterminedValue(v, true, Variable.DeterminationState.Inferred);
                        foreach (var dependent in Variables[v].PositiveClauses)
                            // Dependent is now forced to true
                            counts[dependent] = -1;

                        foreach (var dependent in Variables[v].NegativeClauses)
                        {
                            // Dependent now has one less undetermined literal
                            if (counts[dependent] == 0)
                                // Never got initialized
                                // Note we don't have to decrement because the call below sees that v is not predetermined.
                                counts[dependent] = UndeterminedDisjunctCount(Clauses[dependent]);
                            else
                                counts[dependent]--;
                            if (counts[dependent] == 0)
                                throw new ContradictionException(this, Clauses[dependent]);

                            if (counts[dependent] == 1)
                                walkQueue.Enqueue(Clauses[dependent]);
                        }
                    }
                    else
                    {
                        // Negative literal, so it must be false
                        SetPredeterminedValue(-v, false, Variable.DeterminationState.Inferred);
                        foreach (var dependent in Variables[-v].NegativeClauses)
                            // Dependent is now forced to true
                            counts[dependent] = -1;

                        foreach (var dependent in Variables[-v].PositiveClauses)
                        {
                            // Dependent now has one less undetermined literal
                            if (counts[dependent] == 0)
                                // Never got initialized
                                // Note we don't have to decrement because the call below sees that v is not predetermined.
                                counts[dependent] = UndeterminedDisjunctCount(Clauses[dependent]);
                            else
                                counts[dependent]--;
                            if (counts[dependent] == 0)
                                throw new ContradictionException(this, Clauses[dependent]);
                            if (counts[dependent] == 1)
                                walkQueue.Enqueue(Clauses[dependent]);
                        }
                    }

                    // Take this clause out of commission
                    counts[c.Index] = -1;
                }
            }
            
            foreach (var c in Clauses)
                Walk(c);
            while (walkQueue.Count > 0)
                Walk(walkQueue.Dequeue());
#if PerformanceStatistics
            OptimizationTime = Stopwatch.ElapsedTicks / (0.000001f * Stopwatch.Frequency);
            if (LogPerformanceDataToConsole)
                Console.WriteLine(PerformanceStatistics);
#endif
        }

        /// <summary>
        /// Find the first (and presumably only) undetermined disjunct of the clause.
        /// </summary>
        /// <param name="c">The clause</param>
        /// <returns>Signed index of the disjunct</returns>
        short UndeterminedDisjunctOf(Clause c)
        {
            foreach (var d in c.Disjuncts)
            {
                if (!Variables[Math.Abs(d)].IsPredetermined)
                    return d;
            }
            throw new InvalidOperationException("Internal error - UndeterminedDisjunctOf called on clause with no undertermined disjuncts");
        }

        short CountUndeterminedDisjuncts(Clause c)
        {
            short count = 0;
            foreach (var d in c.Disjuncts)
            {
                if (d > 0)
                {
                    // Positive literal
                    var v = Variables[d];
                    if (v.IsPredetermined)
                    {
                        if (v.IsAlwaysTrue)
                        {
                            // This clause is always pre-satisfied
                            return -1;
                        }
                    }
                    else
                        count++;
                }
                else
                {
                    // Positive literal
                    var v = Variables[-d];
                    if (v.IsPredetermined)
                    {
                        if (v.IsAlwaysFalse)
                        {
                            // This clause is always pre-satisfied
                            return -1;
                        }
                    }
                    else
                        count++;
                }
            }

            if (count == 0)
                throw new ContradictionException(this, c);

            return count;
        }
#else
        /// <summary>
        /// Do a simple constant-folding pass over the program.
        /// This is technically called unit resolution, but it basically means constant folding
        /// </summary>
        public void Optimize()
        {
            FinishCodeGeneration();
            ResetInferredPropositions();

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
                            throw new ContradictionException(this, c);

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
                    if (v.IsPredetermined)
                    {
                        if (v.PredeterminedValue)
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
                    if (v.IsPredetermined)
                    {
                        if (!v.PredeterminedValue)
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
                SetPredeterminedValue(inferred, true, Variable.DeterminationState.Inferred);
            else
                SetPredeterminedValue(-inferred, false, Variable.DeterminationState.Inferred);
            return OptimizationState.Optimized;
        }
#endif
#endregion

#region Manipulation of predetermined values of variables
        /// <summary>
        /// Set the truth value of the proposition across all models, or get the predetermined value.
        /// </summary>
        /// <param name="p">Proposition to check</param>
        /// <returns>Predetermined value</returns>
        // ReSharper disable once UnusedMember.Global
        public bool this[Proposition p]
        {
            get
            {
                if (!Variables[p.Index].IsPredetermined)
                    throw new InvalidOperationException($"{p} does not have a predetermined value; Call Solve() on the problem, and then check for the proposition's value in the solution.");
                return Variables[p.Index].PredeterminedValue;
            }
            set
            {
                if (Variables[p.Index].DeterminionState == Variable.DeterminationState.Fixed
                    && Variables[p.Index].PredeterminedValue != value)
                    throw new InvalidOperationException($"{p}'s value is fixed by an Assertion in the problem.  It cannot be changed.");
                SetPredeterminedValue(p, value, Variable.DeterminationState.Set);
            }
        }
        
        /// <summary>
        /// Find all the propositions that were previously determined through optimization and set
        /// them back to floating status.
        /// </summary>
        private void ResetInferredPropositions()
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].DeterminionState == Variable.DeterminationState.Inferred)
                {
                    var v = Variables[i];
                    v.DeterminionState = Variable.DeterminationState.Floating;
                    Variables[i] = v;
                }
            }

            RecomputeFloatingVariables();
        }

        private void RecomputeFloatingVariables()
        {
            FloatingVariables.Clear();
            for (ushort i = 0; i < Variables.Count; i++)
                if (Variables[i].DeterminionState == Variable.DeterminationState.Floating)
                    FloatingVariables.Add(i);
        }

        /// <summary>
        /// Reset all propositions that had previously been set by the user to floating status.
        /// </summary>
        public void ResetPropositions()
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].DeterminionState == Variable.DeterminationState.Set)
                {
                    var v = Variables[i];
                    v.DeterminionState = Variable.DeterminationState.Floating;
                    Variables[i] = v;
                }
            }
        }
#endregion
    }
}
