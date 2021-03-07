using System;
using System.Collections.Generic;
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
        public readonly bool IsNormalDisjunction;

        protected Constraint(bool isDisjunction, ushort min, short[] disjuncts, int extraHash)
        {
            IsNormalDisjunction = isDisjunction;
            Disjuncts = Disjuncts = disjuncts.Distinct().ToArray();
            Hash = ComputeHash(Disjuncts) ^ extraHash;
            if ((min != 1) && disjuncts.Length != Disjuncts.Length)
                throw new ArgumentException("Nonstandard clause has non-unique disjuncts");
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
        public virtual ushort CountDisjuncts(Solution solution)
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
        /// ThreatCountDelta when current clause is getting one more true disjunct.
        /// </summary>
        public abstract int ThreatCountDeltaIncreasing(ushort count);

        /// <summary>
        /// ThreatCountDelta when current clause is getting one less true disjunct.
        /// </summary>
        public abstract int ThreatCountDeltaDecreasing(ushort count);

        ///<summary>
        /// transit prop appears as a negative literal in clause from false -> true,
        /// OR prop appears as a positive literal in clause from true -> false
        /// </summary>
        public abstract void UpdateTruePositiveAndFalseNegative(BooleanSolver b);

        ///<summary>
        /// transit prop appears as a negative literal in clause from true -> false,
        /// OR prop appears as a positive literal in clause from false -> true
        /// </summary>
        public abstract void UpdateTrueNegativeAndFalsePositive(BooleanSolver b);

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


        /// <summary>
        /// Check if a constraint's condition literal is satisfied.
        /// Conditional PBC only. Default true for all other types of constraint.
        /// </summary>
        public virtual bool IsEnabled(Solution s) => true;
    }
}