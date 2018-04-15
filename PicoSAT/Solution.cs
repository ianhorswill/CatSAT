#define RANDOMIZE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PicoSAT
{
    /// <summary>
    /// The output of a program; a model satisfying the clauses of the Problem.
    /// Note: for packaging reasons, this is also where the actual solver code lives,
    /// rather than in Program.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class Solution
    {
        #region Solver parameters
        /// <summary>
        /// Number of flips of propositions we can try before we give up and start over.
        /// </summary>
        public readonly int MaxFlips;
        /// <summary>
        /// Number of times we can start over before we give up entirely.
        /// </summary>
        public readonly int MaxTries;

        /// <summary>
        /// Probability that the solver will flip a random variable rather than a variable from an unsatisfied clause.
        /// </summary>
        public readonly int RandomFlipProbability;
        #endregion

        #region Solver state
        /// <summary>
        /// The Program for which this is a solution.
        /// </summary>
        public readonly Problem Problem;

        /// <summary>
        /// States of the different propositions of the Program, indexed by proposition number.
        /// </summary>
        private readonly bool[] propositions;

        /// <summary>
        /// Number of presently true disjuncts in each of the Program's clauses, index by clause number.
        /// </summary>
        private readonly ushort[] trueDisjunctCount;

        /// <summary>
        /// Total number of unsatisfied clauses
        /// </summary>
        private readonly List<ushort> unsatisfiedClauses = new List<ushort>();
        #endregion

        internal Solution(Problem problem, int maxFlips, int maxTries, int randomFlipProbability)
        {
            Problem = problem;
            MaxFlips = maxFlips;
            MaxTries = maxTries;
            RandomFlipProbability = randomFlipProbability;
            propositions = new bool[problem.Variables.Count];
            trueDisjunctCount = new ushort[problem.Clauses.Count];
        }

        private string DebuggerDisplay
        {
            // ReSharper disable once UnusedMember.Local
            get
            {
                var b = new StringBuilder();
                var firstOne = true;
                b.Append("<");
                for (int i = 1; i < propositions.Length; i++)
                {
                    if (propositions[i])
                    {
                        if (firstOne)
                            firstOne = false;
                        else
                            b.Append(", ");
                        b.Append(Problem.Variables[i].Proposition);
                    }
                }
                b.Append(">");
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
                return propositions[literal];
            return !propositions[-literal];
        }

        /// <summary>
        /// Test the truth of a proposition/positive literal
        /// </summary>
        /// <param name="index">Index of the proposition</param>
        /// <returns>True if the proposition is true in the model</returns>
        public bool IsTrue(ushort index)
        {
            return propositions[index];
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

        public int Count(IEnumerable<Literal> literals)
        {
            return literals.Count(IsTrue);
        }

        public bool All(IEnumerable<Literal> literals)
        {
            var lits = literals.ToArray();
            return Quantify(lits.Length, lits.Length, lits);
        }

        public bool Exists(IEnumerable<Literal> literals)
        {
            return literals.Any(IsTrue);
        }

        public bool Unique(IEnumerable<Literal> literals)
        {
            return Quantify(1, 1, literals);
        }

        public bool Exactly(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, n, literals);
        }

        public bool AtMost(int n, IEnumerable<Literal> literals)
        {
            return Quantify(0, n, literals);
        }

        public bool AtLeast(int n, IEnumerable<Literal> literals)
        {
            return Quantify(n, 0, literals);
        }
        #endregion
        
        #region Solver
        /// <summary>
        /// Try to find an assignment of truth values to propositions that satisfied the Program.
        /// Implements the WalkSAT algorithm
        /// </summary>
        /// <returns>True if a satisfying assignment was found.</returns>
        internal bool Solve()
        {
            for (var t = MaxTries; t > 0; t--)
            {
                MakeRandomAssignment();

                // TODO: maintain a list of non-constant variables so we never even try to flip constants

                for (var f = MaxFlips; unsatisfiedClauses.Count > 0 && f > 0; f--)
                {
                    if (Random.InRange(100) < RandomFlipProbability)
                        // Flip a completely random variable
                        // This is to pull us out of local minima
                        Flip((ushort)(Random.InRange((uint)propositions.Length)));
                    else
                    {
                        // Hill climb: pick an unsatisfied clause at random and flip one of its variables
                        var targetClause = Problem.Clauses[unsatisfiedClauses.RandomElement()];
                        Flip(LeastBadLiteralToFlip(targetClause.Disjuncts));
                    }
                }

                if (unsatisfiedClauses.Count == 0)
                    return true;
            }
            
            // Give up
            return false;
        }

        /// <summary>
        /// Find the proposition from the specified clause that will do the least damage to the clauses that are already satisfied.
        /// </summary>
        /// <param name="disjuncts">Signed indices of the disjucts of the clause</param>
        /// <returns>Index of the prop to flip</returns>
        private ushort LeastBadLiteralToFlip(short[] disjuncts)
        {
            var bestCount = int.MaxValue;
            var best = 0;
#if RANDOMIZE
            // Walk disjuncts in a reasonably random order
            var dCount = (uint)disjuncts.Length;
            var index = Random.InRange(dCount);
            var prime = Random.Prime();
            for (var i = 0; i < dCount; i++)
            {
                var value = disjuncts[index];
                index = (index + prime) % dCount;
#else
                foreach (var value in disjuncts)
            {
#endif
                var threatCount = ThreatenedClauseCount((ushort)Math.Abs(value));
                if (threatCount == 0)
                    // Fast path = can't do better than this
                    return (ushort) Math.Abs(value);
                if (threatCount < bestCount)
                {
                    best = value;
                    bestCount = threatCount;
                }
            }

            return (ushort)Math.Abs(best);
        }

        /// <summary>
        /// The number of currently satisfied clauses that would become unsatisfied if we flipped this proposition.
        /// </summary>
        /// <param name="pIndex">Index of the proposition</param>
        /// <returns></returns>
        int ThreatenedClauseCount(ushort pIndex)
        {
            var threatCount = 0;
            var prop = Problem.Variables[pIndex];

            if (propositions[pIndex])
            {
                // prop is currently true, so we would be flipping it to false

                // For positive literals the satisfied disjunct count would decrease
                foreach (var cIndex in prop.PositiveClauses)
                {
                    if (Problem.Clauses[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
                        threatCount++;
                }

                // For negative literals, the satisfied disjunct count would increase
                foreach (var cIndex in prop.NegativeClauses)
                {
                    if (Problem.Clauses[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
                        threatCount++;
                }
            }
            else
            {
                // prop is currently false, so we would be flipping it to true

                // For positive literals the satisfied disjunct count would increase
                foreach (var cIndex in prop.PositiveClauses)
                {
                    if (Problem.Clauses[cIndex].OneTooManyDisjuncts((ushort)(trueDisjunctCount[cIndex] + 1)))
                        threatCount++;
                }

                // For negative literals, the satisfied disjunct count would decrease
                foreach (var cIndex in prop.NegativeClauses)
                {
                    if (Problem.Clauses[cIndex].OneTooFewDisjuncts((ushort)(trueDisjunctCount[cIndex] - 1)))
                        threatCount++;
                }
            }

            return threatCount;
        }

        /// <summary>
        /// Flip the variable at the specified index.
        /// </summary>
        /// <param name="pIndex">Index of the variable/proposition to flip</param>
        private void Flip(ushort pIndex)
        {
            var prop = Problem.Variables[pIndex];
            if (prop.IsConstant)
                // Can't flip it.
                return;

            if (propositions[pIndex])
            {
                // Flip true -> false
                propositions[pIndex] = false;

                // Update the clauses in which this appears as a postive literal
                foreach (ushort cIndex in prop.PositiveClauses)
                {
                    // prop appears as a positive literal in clause.
                    // We just made it false, so clause now has fewer satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooManyDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = --trueDisjunctCount[cIndex];
                    if (clause.OneTooFewDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }

                // Update the clauses in which this appears as a negative literal
                foreach (ushort cIndex in prop.NegativeClauses)
                {
                    // prop appears as a negative literal in clause.
                    // We just made it false, so clause now has more satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooFewDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = ++trueDisjunctCount[cIndex];
                    if (clause.OneTooManyDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }
            }
            else
            {
                // Flip false -> true
                propositions[pIndex] = true;

                // Update the clauses in which this appears as a postive literal
                foreach (ushort cIndex in prop.PositiveClauses)
                {
                    // prop appears as a positive literal in clause.
                    // We just made it true, so clause now has more satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooFewDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = ++trueDisjunctCount[cIndex];
                    if (clause.OneTooManyDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }

                // Update the clauses in which this appears as a negative literal
                foreach (ushort cIndex in prop.NegativeClauses)
                {
                    // prop appears as a negative literal in clause.
                    // We just made it true, so clause now has fewer satisfied disjuncts.
                    var clause = Problem.Clauses[cIndex];
                    if (clause.OneTooManyDisjuncts(trueDisjunctCount[cIndex]))
                        // We just satisfied it
                        unsatisfiedClauses.Remove(cIndex);
                    var dCount = --trueDisjunctCount[cIndex];
                    if (clause.OneTooFewDisjuncts(dCount))
                        // It just transitioned from satisfied to unsatisfied
                        unsatisfiedClauses.Add(cIndex);
                }
            }
        }

        /// <summary>
        /// Randomly assign values to the propositions,
        /// and initialize the other state information accordingly.
        /// </summary>
        private void MakeRandomAssignment()
        {
            // Initialize propositions[]
            for (var i = 0; i < propositions.Length; i++)
            {
                propositions[i] = Problem.Variables[i].IsConstant?Problem.Variables[i].ConstantValue:Random.Next() % 2 == 0;
            }

            unsatisfiedClauses.Clear();

            // Initialize trueDisjunctCount[] and unsatisfiedClauses
            for (ushort i = 0; i < trueDisjunctCount.Length; i++)
            {
                var c = Problem.Clauses[i];
                var satisfiedDisjuncts = c.CountDisjuncts(this);
                trueDisjunctCount[i] = satisfiedDisjuncts;
                if (!c.IsSatisfied(satisfiedDisjuncts))
                    unsatisfiedClauses.Add(i);
            }
        }
#endregion
    }
}
