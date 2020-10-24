using System;
using System.Linq;
using System.Text;

namespace CatSAT
{
    internal abstract class Clause
    {
        /// <summary>
        /// The literals of the constraint
        /// </summary>
        internal readonly short[] Disjuncts;

        public readonly int Hash;

        /// <summary>
        /// Position in the Problem's Constraints list.
        /// </summary>
        internal ushort Index;

        /// <summary>
        /// True if this is a plain old boring disjunction
        /// </summary>
        public bool IsNormalDisjunction { get; protected set; }

        /// <summary>
        /// Minimum number of disjuncts that must be true in order for the constraint
        /// to be satisfied, minus one.  For a normal clause, this is 0 (i.e. the real min number is 1)
        /// </summary>
        public readonly short MinDisjunctsMinusOne;



        protected Clause(ushort min, short[] disjuncts, int extraHash)
        {
            Disjuncts = Disjuncts = disjuncts.Distinct().ToArray();
            Hash = ComputeHash(Disjuncts) ^ extraHash;
            if ((min != 1) && disjuncts.Length != Disjuncts.Length)
                throw new ArgumentException("Nonstandard clause has non-unique disjuncts");
            MinDisjunctsMinusOne = (short)(min - 1);
            
        }

        private static int ComputeHash(short[] disjuncts)
        {
            var hash = 0;
            foreach (var disjunct in disjuncts)
            {
                var rotHash = hash << 1 | ((hash >> 31) & 1);
                hash = rotHash ^ disjunct;
            }

            return hash;
        }

        /// <summary>
        /// Return the number of disjuncts that are satisfied in the specified solution (i.e. model).
        /// </summary>
        /// <param name="solution">Solution to test against</param>
        /// <returns>Number of satisfied disjuncts</returns>
        public ushort CountDisjuncts(Solution solution)
        {
            ushort count = 0;
            foreach (var d in Disjuncts)
                if (solution.IsTrue(d))
                    count++;
            return count;
        }

        /// <summary>
        /// Is this constraint satisfied if the specified number of disjuncts is satisfied?
        /// </summary>
        /// <param name="satisfiedDisjuncts">Number of satisfied disjuncts</param>
        /// <returns>Whether the constraint is satisfied.</returns>
        public abstract bool IsSatisfied(ushort satisfiedDisjuncts);

        /// <summary>
        /// Is the specified number of disjuncts one too many for this constraint to be satisfied?
        /// </summary>
        public abstract bool OneTooManyDisjuncts(ushort satisfiedDisjuncts);

        /// <summary>
        /// Is the specified number of disjuncts one too few for this constraint to be satisfied?
        /// </summary>
        public abstract bool OneTooFewDisjuncts(ushort satisfiedDisjuncts);

        /// <summary>
        /// ThreatCountDelta when current clause is getting one more disjunct.
        /// </summary>
        /// <summary>
        /// ThreatCountDelta when current clause is getting one more disjunct.
        /// </summary>
        public int ThreatCountDeltaIncreasing(ushort count)
        {
            int threatCountDelta = 0;
            if (OneTooFewDisjuncts(count))
                threatCountDelta = -1;
            else if (OneTooManyDisjuncts((ushort)(count + 1)))
                threatCountDelta = 1;
            return threatCountDelta;
        }
        /// <summary>
        /// ThreatCountDelta when current clause is getting one less disjunct.
        /// </summary>
        public int ThreatCountDeltaDecreasing(ushort count)
        {
            int threatCountDelta = 0;
            if (OneTooFewDisjuncts((ushort)(count - 1)))
                threatCountDelta = 1;
            else if (OneTooManyDisjuncts(count))
                threatCountDelta = -1;
            return threatCountDelta;
        }
        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public void UpdateTruePositiveAndFalseNegative(BooleanSolver b, ushort cIndex)
        {
            if (OneTooManyDisjuncts(b.TrueDisjunctCount[cIndex]))
                // We just satisfied it
                b.unsatisfiedClauses.Remove(cIndex);
            var dCount = --b.TrueDisjunctCount[cIndex];
            if (OneTooFewDisjuncts(dCount))
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(cIndex);
        }

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public void UpdateTrueNegativeAndFalsePositive(BooleanSolver b, ushort cIndex)
        {
            if (OneTooFewDisjuncts(b.TrueDisjunctCount[cIndex]))
                // We just satisfied it
                b.unsatisfiedClauses.Remove(cIndex);
            var dCount = ++b.TrueDisjunctCount[cIndex];
            if (OneTooManyDisjuncts(dCount))
                // It just transitioned from satisfied to unsatisfied
                b.unsatisfiedClauses.Add(cIndex);
        }
        /// <summary>
        /// Find the proposition from the specified clause that will do the least damage to the clauses that are already satisfied.
        /// </summary>
        /// <param name="b">Current BooleanSolver</param>
        /// <returns>Index of the prop to flip</returns>
        public abstract ushort GreedyFlip(BooleanSolver b);

        internal abstract void Decompile(Problem p, StringBuilder b);

        /// <summary>
        /// Generate a textual representation of the constraint for debugging purposes
        /// </summary>
        internal string Decompile(Problem p)
        {
            var b = new StringBuilder();
            Decompile(p, b);
            return b.ToString();
        }

        /// <summary>
        /// Check if this constraint is just a copy of (or identical to) the specified constraint
        /// </summary>
        internal abstract bool EquivalentTo(Clause c);

    }
}