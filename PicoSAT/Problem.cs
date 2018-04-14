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
            if (Tight)
                CheckTightness();
            CompileRuleBodies();
            var m = new Solution(this, MaxFlips, MaxTries, RandomFlipProbability);
            if (m.Solve())
                return m;
            if (throwOnUnsolvable)
                throw new UnsatisfiableException(this);
            return null;
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
            var v = Variables[p.Index];
            v.SetConstant(value);
            Variables[p.Index] = v;
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

        private void CompileRuleBodies()
        {
            if (compilationState == CompilationState.HaveRules)
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

            compilationState = CompilationState.Compiled;
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
#endregion
    }
}
