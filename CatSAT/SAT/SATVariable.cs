#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SATVariable.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Diagnostics;

namespace CatSAT
{
    /// <summary>
    /// Run-time representation of the truth assignment of a proposition in a Solution.
    /// This is kept separate from the Proposition objects themselves, partly in the hopes of
    /// improving cache performance, and partly so that the Proposition objects can be GC'ed once the
    /// clauses are computed.
    /// 
    /// Note - the actual truth value of the Variable isn't stored here, it's stored in the Solution
    /// object, since we can have multiple solutions that assign different values to the variable.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Proposition) + "}")]
    internal struct SATVariable
    {
        public SATVariable(Proposition proposition)
        {
            Proposition = proposition;
            PositiveClauses = new List<ushort>();
            NegativeClauses = new List<ushort>();
            PredeterminedValue = false;
            DeterminionState = DeterminationState.Floating;
        }

        /// <summary>
        /// The Proposition object for which this variable holds the truth value.
        /// </summary>
        public readonly Proposition Proposition;

        public override string ToString()
        {
            return Proposition.ToString();
        }

        /// <summary>
        /// Clauses in which this variable appears unnegated.
        /// Used to know what clauses to check if we flip this variable
        /// </summary>
        public readonly List<ushort> PositiveClauses;
        /// <summary>
        /// Clauses in which this variable appears negated.
        /// Used to know what clauses to check if we flip this variable
        /// </summary>
        public readonly List<ushort> NegativeClauses;

        /// <summary>
        /// Tracks whether the variable's value is pre-determined, and why
        /// </summary>
        public enum DeterminationState : byte
        {
            /// <summary>
            /// The solver is free to choose this as it likes
            /// </summary>
            Floating,
            /// <summary>
            /// The optimizer determined this from other set or fixed variables
            /// </summary>
            Inferred,
            /// <summary>
            /// The variable's value was forced by the user, but they could change their mind
            /// </summary>
            Set,
            /// <summary>
            /// The variable's value was immutably set by an Assertion.
            /// </summary>
            Fixed
        }

        /// <summary>
        /// Whether the variable's value is predetermined.
        /// If floating, then the solver is allowed to choose its value freely.
        /// If any other value, the solver has to use the value in PredeterminedValue.
        /// </summary>
        public DeterminationState DeterminionState;

        /// <summary>
        /// Whether the value of the variable is fixed.
        /// </summary>
        public bool IsPredetermined => DeterminionState != DeterminationState.Floating;
        
        /// <summary>
        /// Value of variable if it is predetermined
        /// </summary>
        public bool PredeterminedValue;

        public bool IsAlwaysTrue => IsPredetermined && PredeterminedValue;
        public bool IsAlwaysFalse => IsPredetermined && !PredeterminedValue;

        /// <summary>
        /// Randomly chooses an initial state for this SATVariable based on its Proposition's InitialProbability
        /// </summary>
        internal bool RandomInitialState => Random.Float(0, 1) <= Proposition.InitialProbability;
    }
}
