using System;
using System.Linq;
using System.Text;

namespace CatSAT
{
    internal abstract class Constraint
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

        /// <summary>
        /// Maximum number of disjuncts that are allowed to be true in order for the
        /// constraint to be considered satisfied, plus one.
        /// For a normal clause, there is no limit, so this gets set to Disjuncts.Count+1.
        /// </summary>
        public readonly ushort MaxDisjunctsPlusOne;

        protected Constraint(ushort min, ushort max, short[] disjuncts, int extraHash)
        {
            Disjuncts = Disjuncts = disjuncts.Distinct().ToArray();
            Hash = ComputeHash(Disjuncts) ^ extraHash;
            if ((min != 1 || max != 0) && disjuncts.Length != Disjuncts.Length)
                throw new ArgumentException("Nonstandard clause has non-unique disjuncts");
            MinDisjunctsMinusOne = (short)(min - 1);
            MaxDisjunctsPlusOne = (ushort)(max == 0 ? disjuncts.Length + 1 : max + 1);
            IsNormalDisjunction = MinDisjunctsMinusOne == 0 && MaxDisjunctsPlusOne >= Disjuncts.Length + 1;
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
        public bool IsSatisfied(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts > MinDisjunctsMinusOne && satisfiedDisjuncts < MaxDisjunctsPlusOne;
        }

        /// <summary>
        /// Is the specified number of disjuncts one too many for this constraint to be satisfied?
        /// </summary>
        public bool OneTooManyDisjuncts(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts == MaxDisjunctsPlusOne;
        }

        /// <summary>
        /// Is the specified number of disjuncts one too few for this constraint to be satisfied?
        /// </summary>
        public bool OneTooFewDisjuncts(ushort satisfiedDisjuncts)
        {
            return satisfiedDisjuncts == MinDisjunctsMinusOne;
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
        internal abstract bool EquivalentTo(Constraint c);
    }
}