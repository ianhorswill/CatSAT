#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Problem.cs" company="Ian Horswill">
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using static CatSAT.Language;

namespace CatSAT
{
    /// <summary>
    /// A logic program.
    /// Contains a set of propositions, rules for the propositions, and general clauses.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Decompiled) + "}")]
    public class Problem
    {
        //
        // This is basically a storage area for Propositions, Variables, and Constraints (constraints)
        // It maintains bookkeeping information about how they all relate to one another and it
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
            SATVariables.Add(new SATVariable(new Proposition("I am not a valid proposition!  I am a placeholder!", 0)));
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
        /// Number of microseconds spent in Boolean Constraint Propagation
        /// </summary>
        public float OptimizationTime { get; private set; }
        /// <summary>
        /// Number of microseconds spent computing the completions of rules
        /// </summary>
        public float CompilationTime { get; private set; }
        /// <summary>
        /// Number of microseconds spent building the problem object itself.
        /// This is the time spent making propositions and predicates, calling Assert, etc.
        /// It's measured as elapsed time from the calling of the constructor to the calling
        /// of the FinishRuleCompilation() method.
        /// </summary>
        public float CreationTime { get; private set; }

        /// <summary>
        /// Statistics for a single performance measurement
        /// </summary>
        public struct TimingData
        {
            /// <summary>
            /// Shortest time spent on the operation (microseconds)
            /// </summary>
            public float Min;
            /// <summary>
            /// Longest time spent on the operation (microseconds)
            /// </summary>
            public float Max;
            /// <summary>
            /// Sum of total time spent on the operation over all solver operations
            /// </summary>
            private float sum;
            /// <summary>
            /// Number of solver calls over which sum is split
            /// </summary>
            private int count;

            /// <summary>
            /// Average time spent on the operation per solver call (microseconds)
            /// </summary>
            public float Average => count==0?0:sum / count;

            internal void AddReading(float reading)
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
        /// <summary>
        /// Time spent solving
        /// </summary>
        public TimingData SolveTimeMicroseconds;
        /// <summary>
        /// Number of flips used in the solver
        /// </summary>
        // ReSharper disable once UnassignedField.Global
        public TimingData SolveFlips;

        private static string _logFile;
#endif


        /// <summary>
        /// Always print performance data to the console.
        /// </summary>
        public static bool LogPerformanceDataToConsole;

        /// <summary>
        /// File to which to log performance data, if any.
        /// </summary>
        public static string LogFile
        {
#if PerformanceStatistics
            get => _logFile;
            set
            {
                _logFile = value;
                System.IO.File.WriteAllLines(_logFile, new [] {"Name,Create,Compile,Optimize,Constraints,Variables,Floating,SolveMin, SolveMax,SolveAvg,FlipsMin,FlipsMax,FlipsAvg"});
            }
#else
            get => null;
            set => {};
#endif
        }

        [Conditional("PerformanceStatistics")]
        internal void LogPerformanceData()
        {
#if PerformanceStatistics
            System.IO.File.AppendAllLines(LogFile, new []
            {
                $"'{Name}',{CreationTime},{CompilationTime},{OptimizationTime},{Constraints.Count},{SATVariables.Count},{SATVariables.Count(v => v.DeterminationStatus == SATVariable.DeterminationState.Floating)},{SolveTimeMicroseconds.Min},{SolveTimeMicroseconds.Max},{SolveTimeMicroseconds.Average},{SolveFlips.Min},{SolveFlips.Max},{SolveFlips.Average}"
            });
#endif
        }

        /// <summary>
        /// String showing solving performance statistics for this problem object
        /// </summary>
        public string PerformanceStatistics
        {
            get
            {
#if PerformanceStatistics
                return $"Creation: {CreationTime:#,##0}us, Compilation: {CompilationTime:#,##0.##}us, Optimization: {OptimizationTime:#,##0.##}us, C+O: {CompilationTime+OptimizationTime:0}us";
#else
                return "CatSAT compiled without performance measurements";
#endif
            }
        }

        /// <summary>
        /// String showing size statistics for this problem object
        /// </summary>
        public string Stats
        {
            get
            {
                return
                    $"{SATVariables.Count} variables, {SATVariables.Count(v => v.DeterminationStatus == SATVariable.DeterminationState.Floating)} floating, {Constraints.Count} clauses";
            }
        }
        
        /// <summary>
        /// Returns a textual representation of the constraints in the problem.
        /// </summary>
        public string Decompiled
        {
            // ReSharper disable once UnusedMember.Local
            get
            {
                var b = new StringBuilder();
                foreach (var c in Constraints)
                {
                    c.Decompile(this, b);
                    b.AppendLine();
                }

                foreach (var v in SATVariables)
                    if (v.IsPredetermined)
                    {
                        var propositionName = v.Proposition.Name.ToString();
                        b.Append(v.PredeterminedValue ? propositionName : "!" + propositionName);

                        switch (v.DeterminationStatus)
                        {
                            case SATVariable.DeterminationState.Fixed:
                                b.AppendLine(" // fixed");
                                break;

                            case SATVariable.DeterminationState.Inferred:
                                b.AppendLine(" // inferred");
                                break;

                            case SATVariable.DeterminationState.Floating:
                                b.AppendLine(" // floating");
                                break;

                            case SATVariable.DeterminationState.Set:
                                b.AppendLine(" // set");
                                break;

                            default:
                                b.AppendLine(" // unknown DeterminationState !");
                                break;
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
        /// Number of time-steps in history, if fluents are used.
        /// </summary>
        public int TimeHorizon=-1;

        /// <summary>
        /// Hashtable of Call.TrieNodes holding all the different Calls used in this Problem.
        /// This is used to keep from having to constantly allocate new Call objects and then look them up in a hash table
        /// to get the canonical ones. 
        /// </summary>
        internal readonly Dictionary<string, Call.TrieNode> CallTries = new Dictionary<string, Call.TrieNode>();
        
        /// <summary>
        /// The Propositions in the Problem.
        /// </summary>
        private readonly Dictionary<object, Proposition> propositionTable = new Dictionary<object, Proposition>();

        /// <summary>
        /// The Variables in the Problem.
        /// There is one Variable for each Proposition.  And the Solution assigns a truth value to that 
        /// variable.
        /// </summary>
        internal readonly List<SATVariable> SATVariables = new List<SATVariable>();
        /// <summary>
        /// The constraints in the Problem.
        /// Most of these are normal clauses (disjunctions), but other cardinality constraints are possible.
        /// </summary>
        internal readonly List<Constraint> Constraints = new List<Constraint>();

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

        private List<Variable> variables;

        /// <summary>
        /// All Variables (NBSAT, SMT) attached to this Problem.
        /// </summary>
        public IEnumerable<Variable> Variables()
        {
            if (variables != null)
                foreach (var v in variables)
                    yield return v;
        }


        /// <summary>
        /// True if problem contains a variable with the specified name
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool HasVariableNamed(object name)
        {
            // TODO: make this more performant.
            foreach (var v in variables)
                if (v.Name.Equals(name))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the variable with the specified name, if any.
        /// </summary>
        public Variable VariableNamed(object name)
        {
            // TODO: make this more performant.
            foreach (var v in variables)
                if (v.Name.Equals(name))
                    return v;
            return null;
        }

        private BooleanSolver booleanSolver;

        /// <summary>
        /// The BooleanSolver used by this Problem.
        /// </summary>
        public BooleanSolver BooleanSolver
        {
            get
            {
                // TODO: maybe this should be eagerly allocated rather than lazy
                if (booleanSolver != null)
                    return booleanSolver;
                return booleanSolver = new BooleanSolver(this);
            }
        }
        internal Dictionary<Type, TheorySolver> TheorySolvers;

        /// <summary>
        /// Returns the (unique) TheorySolver of type T for this problem, creating it if necessary.
        /// </summary>
        /// <typeparam name="T">The type of theory solver</typeparam>
        /// <returns>The theory solver of type T</returns>
        public T GetSolver<T>() where T: TheorySolver, new()
        {
            if (TheorySolvers == null)
                TheorySolvers = new Dictionary<Type, TheorySolver>();

            if (TheorySolvers.TryGetValue(typeof(T), out TheorySolver t))
                return (T)t;
            var solver = TheorySolver.MakeTheorySolver<T>(this);
            TheorySolvers[typeof(T)] = solver;
            return solver;
        }

        /// <summary>
        /// Add a new variable to the problem
        /// </summary>
        /// <param name="v"></param>
        internal void AddVariable(Variable v)
        {
            if (variables == null)
                variables = new List<Variable>() { v };
            else
                variables.Add(v);
        }

        /// <summary>
        /// Get a Literal that is true iff both arguments are true.
        /// If called twice with the same arguments, returns the same result.
        /// </summary>
        public Literal Conjunction(Literal a, Literal b)
        {
            if (ReferenceEquals(a, b))
                return a;

            if (a.SignedIndex > b.SignedIndex)
                return Conjunction(b, a);

            var name = Call.FromArgs(this, "&", a, b);
            var alreadyDefined = propositionTable.ContainsKey(name);
            var conjunction = GetProposition(name);
            if (!alreadyDefined)
            {
                AddClause(Not(a), Not(b), conjunction);
                AddClause(Not(conjunction), a);
                AddClause(Not(conjunction), b);
            }
            return conjunction;
        }

#endregion

#region Clause management
        /// <summary>
        /// Forcibly add a constraint to the Problem.
        /// </summary>
        internal Clause AddClause(params Literal[] disjuncts)
        {
            return AddClause(1, 0, disjuncts);
        }

        /// <summary>
        /// Forcibly add a constraint to the Problem.
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
        /// Forcibly add a constraint to the Problem.
        /// </summary>
        private void AddClause(Constraint constraint)
        {
            foreach (var c in Constraints)
                if (c.Hash == constraint.Hash && c.EquivalentTo(constraint))
                    return;

            constraint.Index = (ushort)Constraints.Count;
            Constraints.Add(constraint);

            // Add the constraint to the appropriate constraint list for all the propositions that appear in the constraint
            var clauseIndex = (ushort) (Constraints.Count - 1);
            foreach (var lit in constraint.Disjuncts)
            {
                if (lit > 0)
                    SATVariables[lit].PositiveClauses.Add(clauseIndex);
                else
                    SATVariables[-lit].NegativeClauses.Add(clauseIndex);
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
        
#region Preinitialization
        /// <summary>
        /// Delegate used for pre-setting variables in a solution. 
        /// </summary>
        public delegate void PreSetHandler(Problem p);

        /// <summary>
        /// Called to let user preinitialize truth assignments for solution
        /// </summary>
        public event PreSetHandler InitializeTruthAssignment;

        /// <summary>
        /// Method used for running all registered events. 
        /// </summary>
        public void InvokeInitialization()
        {
            InitializeTruthAssignment?.Invoke(this);
        }

        /// <summary>
        /// Change SATVariables marked preinitialized back to floating.
        /// </summary>
        public void ResetPreinitialization()
        {
            for (int i = 0; i < SATVariables.Count; i++)
            {
                if (SATVariables[i].DeterminationStatus == SATVariable.DeterminationState.Preinitialized)
                {
                    var v = SATVariables[i];
                    v.DeterminationStatus = SATVariable.DeterminationState.Floating;
                    v.PredeterminedValue = SATVariables[i].PredeterminedValue;
                    SATVariables[i] = v;
                }
            }

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
            PrepareToSolve();
            var s = new Solution(this);

            if (TheorySolvers != null)
                foreach (var pair in TheorySolvers)
                    pair.Value.PropagatePredetermined(s);

            if (SolveOne(s, Timeout))
                return s;
            if (throwOnUnsolvable)
                throw new TimeoutException(this);
            return null;
        }

        /// <summary>
        /// Perform preprocessing steps for solver:
        /// - Code generation
        /// - Finding floating variables
        /// </summary>
        private void PrepareToSolve()
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
        }

        /// <summary>
        /// Generate one solution in the specified solution object.
        /// Does not perform preprocessing
        /// </summary>
        /// <param name="s">Solution object to write result into</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>True if solution was found</returns>
        private bool SolveOne(Solution s, int timeout)
        {
            // Note: this also calls the theory solver(s), if needed
            if (BooleanSolver.Solve(s, timeout, out _))
            {
#if PerformanceStatistics
                SolveTimeMicroseconds.AddReading(BooleanSolver.SolveTimeMicroseconds);
                SolveFlips.AddReading(BooleanSolver.SolveFlips);
                if (LogPerformanceDataToConsole)
                    Console.WriteLine(BooleanSolver.PerformanceStatistics);
#endif
                // Write back values of variables
                foreach (var v in Variables())
                    if (v.IsDefinedInInternal(s))
                        s.SetTheoryVariableValue(v, v.ValueInternal(s));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Makes a best effort to find a solution with a high utility as specified by the utilities
        /// of this problem's propositions.
        /// </summary>
        /// <param name="flips">Maximum number of flips to spend trying to find better solutions</param>
        /// <param name="throwOnUnsolvable">Throw an exception if no solution is found.  Otherwise returns null when no solution is found.</param>
        /// <returns>Best solution system could find in the time available, or null if it could find no solution.</returns>
        public Solution HighUtilitySolution(int flips, bool throwOnUnsolvable = true)
        {
            PrepareToSolve();
            Solution best = null;
            var s = new Solution(this);
            while (flips > 0 && BooleanSolver.Solve(s, flips, out var unused, best != null))
            {
                // Got a solution; see if it's better than the current best.
                if (best == null)
                {
                    best = new Solution(this);
                    best.CopyFrom(s);
                } else if (s.Utility > best.Utility)
                    best.CopyFrom(s);

                if (!BooleanSolver.ImproveUtility(5))
                    // We have an optimal solution
                    return best;
                flips = unused;
            }

            if (best == null && throwOnUnsolvable)
                throw new TimeoutException(this);
            return best;
        }

        #region Assertions
        /// <summary>
        /// Adds a set of assertions to this problem.
        /// Assertions are immutable: they cannot be changed or reset
        /// Assertions cannot be added after the first call to Solve()
        /// </summary>
        /// <param name="assertions">Assertions to add (literals, rules, implications, etc.)</param>
        /// <exception cref="InvalidOperationException">When the Problem has already been solved once</exception>
        public void Assert(params Assertable[] assertions)
        {
            foreach (var a in assertions)
                a.Assert(this);
        }

        /// <summary>
        /// Asserts the literal must always be true in any solution.
        /// Assertions are immutable: they cannot be changed or reset
        /// Assertions cannot be added after the first call to Solve()
        /// </summary>
        /// <param name="literal">Literal tha must always be true</param>
        /// <exception cref="InvalidOperationException">When the Problem has already been solved once</exception>
        public void Assert(Literal literal)
        {
            if (Equals(literal, Proposition.True))
                // We already know that true is true.
                return;
            if (Equals(literal, Proposition.False))
                throw new InvalidOperationException("Attempt to Assert the false proposition.");
            //AddClause(new Clause(1, 0, new[] {l.SignedIndex}));
            switch (literal)
            {
                case Proposition p:
                    SetPredeterminedValue(p, true, SATVariable.DeterminationState.Fixed);
                    break;

                case Negation n:
                    SetPredeterminedValue(n.Proposition, false, SATVariable.DeterminationState.Fixed);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown literal type: {literal.GetType().Name}");
            }
        }

        internal void SetPredeterminedValue(Proposition p, bool value, SATVariable.DeterminationState s)
        {
            SetPredeterminedValue(p.Index, value, s);
        }

        internal void SetPredeterminedValue(int index, bool value, SATVariable.DeterminationState s)
        {
            var v = SATVariables[index];
            v.DeterminationStatus = s;
            v.PredeterminedValue = value;
            SATVariables[index] = v;
        }

        /// <summary>
        /// Declares a value for the proposition that has been inferred as part of the optimization process.
        /// </summary>
        public void SetInferredValue(Proposition p, bool value)
        {
            SetPredeterminedValue(p, value, SATVariable.DeterminationState.Inferred);
        }

        /// <summary>
        /// Used to specify an initial value to be used by the solver for this one particular run of the solver.
        /// </summary>
        /// <param name="p">Proposition to assign a value to</param>
        /// <param name="value">Truth value to assign to the proposition</param>
        public void SetPreinitializedValue(Proposition p, bool value)
        {
            SetPredeterminedValue(p, value, SATVariable.DeterminationState.Preinitialized);
        }

        /// <summary>
        /// Asserts that the head must be true in any solution in which the body is true
        /// Assertions are immutable: they cannot be changed or reset
        /// Assertions cannot be added after the first call to Solve()
        /// </summary>
        /// <param name="implication">Implication that must be true in any solution</param>
        /// <exception cref="InvalidOperationException">When the Problem has already been solved once</exception>
        public void Assert(Implication implication)
        {
            var h = implication.Head;
            if (h is Proposition p && p.IsConstant)
            {
                if (!(bool)p)
                    AddClause(CompileNegatedConjunction(implication.Body));
                // Otherwise h is always true, so don't bother adding a constraint
                return;
            }
            AddClause(CompileImplication(implication));
        }

        /// <summary>
        /// Adds a rule to the problem.
        /// Assertions are immutable: they cannot be changed or reset
        /// Assertions cannot be added after the first call to Solve()
        /// </summary>
        /// <param name="rule">Rule to add to the problem</param>
        /// <exception cref="InvalidOperationException">When the Problem has already been solved once</exception>
        public void Assert(Rule rule)
        {
            if (compilationState == CompilationState.Compiled)
                throw new InvalidOperationException("Can't add rules after calling Solve().");

            if (rule.Head.IsConstant)
            {
                throw new InvalidOperationException("Rule heads cannot be constants.");
            }

            foreach (var d in rule.Body.PositiveLiterals)
                if (!d.IsConstant)
                    rule.Head.AddDependency(d);
            
            rule.Head.AddRuleBody(rule.Body);
            compilationState = CompilationState.HaveRules;
        }

        /// <summary>
        /// Asserts that in any solution, either the head and body must both be true or both be false
        /// Assertions are immutable: they cannot be changed or reset
        /// Assertions cannot be added after the first call to Solve()
        /// </summary>
        /// <param name="equivalence">Equivalence that must be true in any solution</param>
        /// <exception cref="InvalidOperationException">When the Problem has already been solved once</exception>
        public void Assert(Biconditional equivalence)
        {
            // Compile the forward implication
            var disjuncts = DisjunctsFromImplication(equivalence.Head, equivalence.Body);
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
            if (compilationState != CompilationState.Compiled)
            {
#if PerformanceStatistics
                CreationTime = Stopwatch.ElapsedTicks / (0.000001f * Stopwatch.Frequency);
                Stopwatch.Reset();
                Stopwatch.Start();
#endif
                if (compilationState == CompilationState.HaveRules)
                {
                    if (Tight)
                        CheckTightness();
                    CompileRuleBodies();
                }

                if (TheorySolvers != null)
                    foreach (var p in TheorySolvers)
                    {
                        var message = p.Value.Preprocess();
                        if (message != null)
                            throw new ContradictionException(this, message);
                    }
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

            for (int i = 0; i < SATVariables.Count; i++)
                Walk(SATVariables[i].Proposition);
        }

        private void CompileRuleBodies()
        {
            int startingVariableCount = SATVariables.Count;

            for (int i = 0; i < startingVariableCount; i++)
            {
                var v = SATVariables[i];
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
            foreach (var v in SATVariables)
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
        /// <summary>
        /// Declare that these can't simultaneously be true
        /// </summary>
        /// <param name="lits">outlawed literals</param>
        public void Inconsistent(params Literal[] lits)
        {
            AddClause(new Clause(1, 0, lits.Select(l => (short)(-l.SignedIndex)).Distinct().ToArray()));
        }

        /// <summary>
        /// Declare that these can't simultaneously be true 
        /// </summary>
        /// <typeparam name="T">Type of the domain</typeparam>
        /// <param name="domain">Collection to quantify over</param>
        /// <param name="f">Forms a literal from a domain element</param>
        // ReSharper disable once UnusedMember.Global
        public void Inconsistent<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            Inconsistent(domain.Select(f));
        }

        /// <summary>
        /// Declare that these can't simultaneously be true
        /// </summary>
        /// <param name="lits">outlawed literals</param>
        public void Inconsistent(IEnumerable<Literal> lits)
        {
            var enumerable = lits as Literal[] ?? lits.ToArray();
            foreach (var l in enumerable)
                l.BaseProposition.IsQuantified = true;

            AddClause(new Clause(1, 0, enumerable.Select(l => (short)(-l.SignedIndex)).Distinct().ToArray()));
        }

        /// <summary>
        /// Bounds on the number of literals in the specified set that must be true
        /// </summary>
        /// <typeparam name="T">Type of the domain elements</typeparam>
        /// <param name="min">Minimum number of literals that must be true in a solution</param>
        /// <param name="max">Maximum number of literals that may be true in a solution</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function mapping a domain element to a literal</param>
        public void Quantify<T>(int min, int max, IEnumerable<T> domain, Func<T, Literal> f)
        {
            Quantify(min, max, domain.Select(f));
        }

        /// <summary>
        /// Bounds on the number of literals in the specified set that must be true
        /// </summary>
        /// <param name="min">Minimum number of literals that must be true in a solution</param>
        /// <param name="max">Maximum number of literals that may be true in a solution</param>
        /// <param name="literals">Literals being quantified</param>
        // ReSharper disable once UnusedMember.Global
        public void Quantify(int min, int max, params Literal[] literals)
        {
            Quantify(min, max, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Bounds on the number of literals in the specified set that must be true
        /// </summary>
        /// <param name="min">Minimum number of literals that must be true in a solution</param>
        /// <param name="max">Maximum number of literals that may be true in a solution</param>
        /// <param name="literals">Literals being quantified</param>
        public void Quantify(int min, int max, IEnumerable<Literal> literals)
        {
            // TODO: change all this so that max=0 doesn't mean no upper bound; it's turning out ot be a bad design decision.
            if (max > 0 && min > max)
                throw new ContradictionException(this, "minimum number of disjuncts is more than the maximum number");
            var trueCount = 0;
            var set = new HashSet<Literal>();
            var enumerable = literals as Literal[] ?? literals.ToArray();
            foreach (var l in enumerable)
                l.BaseProposition.IsQuantified = true;

            foreach (var l in enumerable)
            {
                if (ReferenceEquals(l, Proposition.True))
                    trueCount++;
                else if (!ReferenceEquals(l, Proposition.False))
                    set.Add(l);
            }

            if (min - trueCount > set.Count)
                throw new ContradictionException(this, "Minimum in quantification is larger than the number of non-false elements in the constraint");

            if (max > 0 && trueCount > max)
                throw new ContradictionException(this, "Quantification constraint can never be satisfied");

            if (max > 0 && max == trueCount)
            {
                foreach (var l in set)
                    Assert(Not(l));
            }
            else if (min - trueCount == set.Count)
            {
                foreach (var l in set)
                    Assert(l);
            }
            else
                Quantify(Math.Max(0, min-trueCount), max==0?0:max-trueCount, set.Select(l => l.SignedIndex).ToArray());
        }

        internal void Quantify(int min, int max, short[] disjuncts)
        {
            AddClause(new Clause((ushort)min, (ushort)max, disjuncts));
        }

        /// <summary>
        /// Assert all the literals must be true
        /// </summary>
        /// <param name="literals">Literals that must be true</param>
        public void All(params Literal[] literals)
        {
            All((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Assert all the literals must be true
        /// </summary>
        public void All<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            All(domain.Select(f));
        }

        /// <summary>
        /// Assert all the literals must be true
        /// </summary>
        /// <param name="literals">Literals that must be true</param>
        // ReSharper disable once UnusedMember.Global
        public void All(IEnumerable<Literal> literals)
        {
            var disjuncts = literals.Select(l => l.SignedIndex).ToArray();
            Quantify(disjuncts.Length, disjuncts.Length, disjuncts);
        }

        /// <summary>
        /// Asserts at least one of the literals must be true in any solution
        /// </summary>
        /// <param name="literals">Literals being quantified</param>
        // ReSharper disable once UnusedMember.Global
        public void Exists(params Literal[] literals)
        {
            Exists((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Asserts at least one of the literals must be true in any solution
        /// </summary>
        public void Exists<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            Exists(domain.Select(f));
        }

        /// <summary>
        /// Asserts at least one of the literals must be true in any solution
        /// </summary>
        /// <param name="literals">Literals being quantified</param>
        // ReSharper disable once UnusedMember.Global
        public void Exists(IEnumerable<Literal> literals)
        {
            Quantify(1, 0, literals);
        }

        /// <summary>
        /// Asserts exactly one of the literals must be true in any solution
        /// </summary>
        /// <param name="literals">Literals being quantified</param>
        public void Unique(params Literal[] literals)
        {
            Unique((IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Asserts exactly one of the literals must be true in any solution
        /// </summary>
        public void Unique<T>(IEnumerable<T> domain, Func<T, Literal> f)
        {
            Unique(domain.Select(f));
        }
        
        /// <summary>
        /// Asserts exactly one of the literals must be true in any solution
        /// </summary>
        /// <param name="literals">Literals being quantified</param>
        public void Unique(IEnumerable<Literal> literals)
        {
            Quantify(1, 1, literals);
        }

        /// <summary>
        /// Asserts exactly N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Number of literals that must be true</param>
        /// <param name="literals">Literals being quantified</param>
        // ReSharper disable once UnusedMember.Global
        public void Exactly(int n, params Literal[] literals)
        {
            Exactly(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Asserts exactly N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Number of literals that must be true</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function mapping domain element to literal</param>
        // ReSharper disable once UnusedMember.Global
        public void Exactly<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            Exactly(n, domain.Select(f));
        }

        /// <summary>
        /// Asserts exactly N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Number of literals that must be true</param>
        /// <param name="literals">Literals being quantified</param>
        public void Exactly(int n, IEnumerable<Literal> literals)
        {
            Quantify(n, n, literals);
        }

        /// <summary>
        /// Asserts at most N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Maximum number of literals that may be true</param>
        /// <param name="literals">Literals being quantified</param>
        public void AtMost(int n, params Literal[] literals)
        {
            AtMost(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Asserts at most N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Maximum number of literals that may be true</param>
        /// <param name="literals">Literals being quantified</param>
        public void AtMost(int n, IEnumerable<Literal> literals)
        {
            Quantify(0, n, literals);
        }

        /// <summary>
        /// Asserts at most  N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Maximum number of literals that may be true</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function mapping domain element to literal</param>
        public void AtMost<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            AtMost(n, domain.Select(f));
        }

        /// <summary>
        /// Asserts at least N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Minimum number of literals that must be true</param>
        /// <param name="literals">Literals being quantified</param>
        public void AtLeast(int n, params Literal[] literals)
        {
            AtLeast(n, (IEnumerable<Literal>)literals);
        }

        /// <summary>
        /// Asserts at least N of the literals must be true in any solution
        /// </summary>
        /// <param name="n">Minimum number of literals that must be true</param>
        /// <param name="domain">Domain over which to quantify</param>
        /// <param name="f">Function mapping domain element to literal</param>
        // ReSharper disable once UnusedMember.Global
        public void AtLeast<T>(int n, IEnumerable<T> domain, Func<T, Literal> f)
        {
            AtLeast(n, domain.Select(f));
        }

        /// <summary>
        /// Asserts at least N of the literals must be true in any solution
        /// </summary>
        /// <param name="literals">Literals being quantified</param>
        /// <param name="n">Minimum number of literals that must be true</param>
        // ReSharper disable once UnusedMember.Global
        public void AtLeast(int n, IEnumerable<Literal> literals)
        {
            Quantify(n, 0, literals);
        }
#endregion

#region Mapping between Literals objects and SATVariables
        /// <summary>
        /// True if the problem has a proposition with the specified name
        /// </summary>
        public bool HasPropositionNamed(object name)
        {
            return propositionTable.ContainsKey(name);
        }

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
            p = new Proposition(key, (ushort) SATVariables.Count);
            SATVariables.Add(new SATVariable(p));
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

        /// <summary>
        /// Returns the Proposition of with the specified name and type from within the Problem, creating one if necessary.
        /// You should probably only be using this if you're writing your own theory solver.
        /// </summary>
        /// <param name="name">Name of the proposition</param>
        /// <typeparam name="T">Type of the proposition</typeparam>
        public T GetSpecialProposition<T>(object name) where T: SpecialProposition, new()
        {
            // It's already in the table
            if (propositionTable.TryGetValue(name, out Proposition old))
                return (T)old;

            // It's a new proposition
            var p = new T() { Name = name, Index = (ushort)SATVariables.Count };
            p.Initialize(this);
            SATVariables.Add(new SATVariable(p));
            propositionTable[name] = p;
            return p;
        }

        private readonly Dictionary<Proposition, Negation> negationTable = new Dictionary<Proposition, Negation>();

        /// <summary>
        /// Returns the (unique) negation literal of this proposition, creating one if necessary.
        /// </summary>
        /// <param name="proposition">Proposition to negate</param>
        public Negation Negation(Proposition proposition)
        {
            if (negationTable.TryGetValue(proposition, out Negation p))
                return p;
            p = new Negation(proposition);
            negationTable[proposition] = p;
            return p;
        }

        internal Proposition KeyOf(Clause clause, ushort position)
        {
            return SATVariables[clause.Disjuncts[position]].Proposition;
        }

        /// <summary>
        /// True if proposition is known to be true in all models
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsAlwaysTrue(Proposition p)
        {
            return SATVariables[p.Index].IsAlwaysTrue;
        }

        /// <summary>
        /// True if proposition is known to be false in all models
        /// </summary>
        public bool IsAlwaysFalse(Proposition p)
        {
            return SATVariables[p.Index].IsAlwaysFalse;
        }

        /// <summary>
        /// True if proposition is known to have the same value in all models
        /// </summary>
        public bool IsConstant(Proposition p)
        {
            return SATVariables[p.Index].IsPredetermined;
        }

        /// <summary>
        /// True if this is a Literal with a fix truth value across all Problems and Solutions.
        /// The only constant Literals are Proposition.True, Proposition.False, and their negations.
        /// </summary>
        /// <param name="lit"></param>
        // ReSharper disable once UnusedMember.Global
        public bool IsConstant(Literal lit)
        {
            switch (lit)
            {
                case Proposition p:
                    return IsConstant(p);

                case Negation n:
                    return IsConstant(n.Proposition);

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Test if this proposition is predetermined (explicitly set, inferred, etc.)
        /// </summary>
        /// <param name="p"></param>
        public bool IsPredetermined(Proposition p)
        {
            return SATVariables[p.Index].IsPredetermined;
        }
#endregion

#region Optimization (unit resolution)
        /// <summary>
        /// Performs unit resolution, aka Boolean constraint propagation aka constant folding on the clauses of the problem.
        /// </summary>
        /// <exception cref="ContradictionException">If the optimizer determines the problem is provably unsatisfiable</exception>
        public void Optimize()
        {
            FinishCodeGeneration();
#if PerformanceStatistics
            Stopwatch.Reset();
            Stopwatch.Start();
#endif
            ResetInferredPropositions();

            // The number of literals in constraint whose values aren't yet known.
            // Or -1 if this constraint now compile-time true.
            var counts = new short[Constraints.Count];

            short UndeterminedDisjunctCount(Constraint c)
            {
                if (counts[c.Index] != 0)
                    return counts[c.Index];
                if (!c.IsNormalDisjunction)
                    // Ignore this constraint
                    return counts[c.Index] = -1;
                var count = CountUndeterminedDisjuncts(c);
                counts[c.Index] = count;
                return count;
            }

            var walkQueue = new Queue<Constraint>();

            void Walk(Constraint c)
            {
                var d = UndeterminedDisjunctCount(c);
                if (d == 1)
                {
                    // All the disjuncts but one are known to be false, so the last must be true.
                    var v = UndeterminedDisjunctOf(c);
                    if (v > 0)
                    {
                        // Positive literal, so it must be true
                        SetPredeterminedValue(v, true, SATVariable.DeterminationState.Inferred);
                        foreach (var dependent in SATVariables[v].PositiveClauses)
                            // Dependent is now forced to true
                            counts[dependent] = -1;

                        foreach (var dependent in SATVariables[v].NegativeClauses)
                        {
                            // Dependent now has one less undetermined literal
                            if (counts[dependent] == 0)
                                // Never got initialized
                                // Note we don't have to decrement because the call below sees that v is not predetermined.
                                counts[dependent] = UndeterminedDisjunctCount(Constraints[dependent]);
                            else
                                counts[dependent]--;
                            if (counts[dependent] == 0)
                                throw new ContradictionException(this, Constraints[dependent]);

                            if (counts[dependent] == 1)
                                walkQueue.Enqueue(Constraints[dependent]);
                        }
                    }
                    else
                    {
                        // Negative literal, so it must be false
                        SetPredeterminedValue(-v, false, SATVariable.DeterminationState.Inferred);
                        foreach (var dependent in SATVariables[-v].NegativeClauses)
                            // Dependent is now forced to true
                            counts[dependent] = -1;

                        foreach (var dependent in SATVariables[-v].PositiveClauses)
                        {
                            // Dependent now has one less undetermined literal
                            if (counts[dependent] == 0)
                                // Never got initialized
                                // Note we don't have to decrement because the call below sees that v is not predetermined.
                                counts[dependent] = UndeterminedDisjunctCount(Constraints[dependent]);
                            else
                                counts[dependent]--;
                            if (counts[dependent] == 0)
                                throw new ContradictionException(this, Constraints[dependent]);
                            if (counts[dependent] == 1)
                                walkQueue.Enqueue(Constraints[dependent]);
                        }
                    }

                    // Take this constraint out of commission
                    counts[c.Index] = -1;
                }
            }
            
            foreach (var c in Constraints)
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
        /// Find the first (and presumably only) undetermined disjunct of the constraint.
        /// </summary>
        /// <param name="c">The constraint</param>
        /// <returns>Signed index of the disjunct</returns>
        short UndeterminedDisjunctOf(Constraint c)
        {
            foreach (var d in c.Disjuncts)
            {
                if (!SATVariables[Math.Abs(d)].IsPredetermined)
                    return d;
            }
            throw new InvalidOperationException("Internal error - UndeterminedDisjunctOf called on constraint with no undetermined disjuncts");
        }

        short CountUndeterminedDisjuncts(Constraint c)
        {
            short count = 0;
            foreach (var d in c.Disjuncts)
            {
                if (d > 0)
                {
                    // Positive literal
                    var v = SATVariables[d];
                    if (v.IsPredetermined)
                    {
                        if (v.IsAlwaysTrue)
                        {
                            // This constraint is always pre-satisfied
                            return -1;
                        }
                    }
                    else
                        count++;
                }
                else
                {
                    // Positive literal
                    var v = SATVariables[-d];
                    if (v.IsPredetermined)
                    {
                        if (v.IsAlwaysFalse)
                        {
                            // This constraint is always pre-satisfied
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
#endregion

#region Manipulation of predetermined values of variables
        /// <summary>
        /// Gets or sets the predetermined value of the proposition.
        /// </summary>
        /// <param name="p">Proposition to check</param>
        /// <returns>Predetermined value</returns>
        /// <exception cref="InvalidOperationException">If the proposition has not been given a predetermined value</exception>
        // ReSharper disable once UnusedMember.Global
        public bool this[Proposition p]
        {
            get
            {
                if (!SATVariables[p.Index].IsPredetermined)
                    throw new InvalidOperationException($"{p} does not have a predetermined value; Call Solve() on the problem, and then check for the proposition's value in the solution.");
                return SATVariables[p.Index].PredeterminedValue;
            }
            set
            {
                if (SATVariables[p.Index].DeterminationStatus == SATVariable.DeterminationState.Fixed
                    && SATVariables[p.Index].PredeterminedValue != value)
                    throw new InvalidOperationException($"{p}'s value is fixed by an Assertion in the problem.  It cannot be changed.");
                SetPredeterminedValue(p, value, SATVariable.DeterminationState.Set);
            }
        }

        /// <summary>
        /// Gets or sets the predetermined value of the literal.
        /// </summary>
        /// <param name="l">Literal to check</param>
        /// <returns>Predetermined value</returns>
        /// <exception cref="InvalidOperationException">If the literal's proposition has not been given a predetermined value</exception>
        // ReSharper disable once UnusedMember.Global
        public bool this[Literal l]
        {
            get
            {
                switch (l)
                {
                    case Proposition p:
                        return this[p];

                    case Negation n:
                        return !this[n.Proposition];

                    default:
                        throw new ArgumentException($"Invalid literal type {l}");
                }
            }

            set
            {
                switch (l)
                {
                    case Proposition p:
                        this[p] = value;
                        break;

                    case Negation n:
                        this[n.Proposition] = !value;
                        break;

                    default:
                        throw new ArgumentException($"Invalid literal type {l}");
                }
            }
        }
        
        /// <summary>
        /// Find all the propositions that were previously determined through optimization and set
        /// them back to floating status.
        /// </summary>
        private void ResetInferredPropositions()
        {
            for (int i = 0; i < SATVariables.Count; i++)
            {
                if (SATVariables[i].DeterminationStatus == SATVariable.DeterminationState.Inferred)
                {
                    var v = SATVariables[i];
                    v.DeterminationStatus = SATVariable.DeterminationState.Floating;
                    SATVariables[i] = v;
                }
            }

            RecomputeFloatingVariables();
        }

        private void RecomputeFloatingVariables()
        {
            FloatingVariables.Clear();
            for (ushort i = 0; i < SATVariables.Count; i++)
                if (SATVariables[i].DeterminationStatus == SATVariable.DeterminationState.Floating)
                    FloatingVariables.Add(i);
        }

        /// <summary>
        /// Reset all propositions that had previously been set by the user to floating status.
        /// </summary>
        public void ResetPropositions()
        {
            for (int i = 0; i < SATVariables.Count; i++)
            {
                if (SATVariables[i].DeterminationStatus == SATVariable.DeterminationState.Set)
                {
                    var v = SATVariables[i];
                    v.DeterminationStatus = SATVariable.DeterminationState.Floating;
                    SATVariables[i] = v;
                }
            }
        }

        /// <summary>
        /// Reset a previously set proposition ot an undetermined state.
        /// </summary>
        /// <param name="p">Proposition to reset</param>
        public void ResetProposition(Proposition p)
        {
            int i = p.Index;
            if (SATVariables[i].DeterminationStatus == SATVariable.DeterminationState.Set)
            {
                var v = SATVariables[i];
                v.DeterminationStatus = SATVariable.DeterminationState.Floating;
                SATVariables[i] = v;
            }
        }
        #endregion
    }
}
