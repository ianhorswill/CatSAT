#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PseudoSMT.cs" company="Ian Horswill">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatSAT;

namespace Tests
{
    [TestClass]
    public class PseudoSMT
    {
        [TestInitialize]
        public void StartLogging()
        {
            Problem.LogPerformanceDataToConsole = true;
            Problem.LogFile = "../../../Test timings.csv";
        }

        [DebuggerDisplay("{name}")]
        class SMTVar
        {
            private readonly string name;
            public float Value;
            public readonly float DefaultMin;
            public readonly float DefaultMax;
            private float currentMin;
            private float currentMax;

            public override string ToString()
            {
                return name;
            }

            public static void Reset()
            {
                AllVars.Clear();
                AllInequalities.Clear();
            }

            /// <summary>
            /// List of variables that this has to be greater than
            /// </summary>
            private readonly List<SMTVar> upperBoundees = new List<SMTVar>();
            /// <summary>
            /// List of variables that this has to be less than
            /// </summary>
            private readonly List<SMTVar> lowerBoundees = new List<SMTVar>();

            private static readonly List<SMTVar> AllVars = new List<SMTVar>();
            private static readonly List<Proposition> AllInequalities = new List<Proposition>();

            public SMTVar(string name, float defaultMin, float defaultMax)
            {
                DefaultMin = defaultMin;
                DefaultMax = defaultMax;
                this.name = name;
                AllVars.Add(this);
            }

            public static bool Solve(Solution s)
            {
                for (int i = 0; i < 50; i++)
                    if (TryOnce(s))
                        return true;
                return false;
            }

            private static bool TryOnce(Solution s)
            {
                foreach (var v in AllVars)
                    v.Initialize();

                if (!FindDependencies(s)) return false;

                foreach (var v in AllVars)
                {
                    float value = CatSAT.Random.Float(v.currentMin, v.currentMax);
                    v.Value = value;
                    if (!v.BoundAbove(value))
                        return false;
                    if (!v.BoundBelow(value))
                        return false;
                }
                
                return true;
            }

            private static bool FindDependencies(Solution s)
            {
                foreach (var c in AllInequalities)
                {
                    if (!s[c])
                        continue;
                    var exp = c.Name as Call;
                    Debug.Assert(exp != null, nameof(exp) + " != null");
                    var v = (SMTVar) (exp.Args[0]);
                    switch (exp.Args[1])
                    {
                        case SMTVar boundingVar:
                            switch (exp.Name)
                            {
                                case ">":
                                    v.upperBoundees.Add(boundingVar);
                                    boundingVar.lowerBoundees.Add(v);
                                    if (!v.BoundBelow(boundingVar.currentMin))
                                        return false;
                                    if (!boundingVar.BoundAbove(v.currentMax))
                                        return false;
                                    break;

                                case "<":
                                    v.lowerBoundees.Add(boundingVar);
                                    boundingVar.upperBoundees.Add(v);
                                    if (!v.BoundAbove(boundingVar.currentMax))
                                        return false;
                                    if (!boundingVar.BoundBelow(v.currentMin))
                                        return false;
                                    break;

                                default:
                                    throw new Exception($"Unknown constraint {exp.Name}");
                            }

                            break;

                        case float bound:
                            switch (exp.Name)
                            {
                                case ">":
                                    v.currentMin = Math.Max(v.currentMin, bound);
                                    break;

                                case "<":
                                    v.currentMax = Math.Min(v.currentMax, bound);
                                    break;

                                default:
                                    throw new Exception($"Unknown constraint {exp.Name}");
                            }

                            if (v.currentMin > v.currentMax)
                                return false;
                            break;
                    }
                }

                return true;
            }

            private void Initialize()
            {
                currentMin = DefaultMin;
                currentMax = DefaultMax;
                upperBoundees.Clear();
                lowerBoundees.Clear();
            }

            /// <summary>
            /// Apply an upper bound.  If it's tighter than our current bound, propagate.
            /// </summary>
            /// <param name="bound">Upper bound - may or may not be lower than CurrentMax</param>
            /// <returns>False if this produces a contradiction</returns>
            bool BoundAbove(float bound)
            {
                if (bound < currentMax)
                {
                    if (bound < currentMin)
                        return false;
                    currentMax = bound;
                    foreach (var d in upperBoundees)
                    {
                        if (!d.BoundAbove(bound))
                            return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Apply an lower bound.  If it's tighter than our current bound, propagate.
            /// </summary>
            /// <param name="bound">Lower bound - may or may not be greater than CurrentMin</param>
            /// <returns>False if this produces a contradiction</returns>
            bool BoundBelow(float bound)
            {
                if (bound > currentMin)
                {
                    if (bound > currentMax)
                        return false;
                    currentMin = bound;
                    foreach (var d in lowerBoundees)
                    {
                        if (!d.BoundBelow(bound))
                            return false;
                    }
                }

                return true;
            }

            public static Proposition operator >(SMTVar a, SMTVar b)
            {
                return SMTProposition(Call.FromArgs(Problem.Current, ">", a, b));
            }

            public static Proposition operator >(SMTVar a, float bound)
            {
                return SMTProposition(Call.FromArgs(Problem.Current, ">", a, bound));
            }

            public static Proposition operator <(SMTVar a, SMTVar b)
            {
                return SMTProposition(Call.FromArgs(Problem.Current, "<", a, b));
            }

            public static Proposition operator <(SMTVar a, float bound)
            {
                return SMTProposition(Call.FromArgs(Problem.Current, "<", a, bound));
            }

            private static Proposition SMTProposition(Call call)
            {
                var prop = Problem.Current.GetProposition(call);
                AllInequalities.Add(prop);
                return prop;
            }
        }

        [TestMethod]
        public void SolveOneVar()
        {
            SMTVar.Reset();
            var x = new SMTVar("x", 0, 10);
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(SMTVar.Solve(null));
                AssertValid(x);
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertValid(SMTVar x)
        {
            Assert.IsTrue(x.Value >= x.DefaultMin);
            Assert.IsTrue(x.Value <= x.DefaultMax);
        }

        [TestMethod]
        public void SolveTwoIndependent()
        {
            SMTVar.Reset();
            var x = new SMTVar("x", 0, 10);
            var y = new SMTVar("y", 0, 10);
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(SMTVar.Solve(null));
                AssertValid(x);
                AssertValid(y);
            }
        }

        [TestMethod]
        public void SolveTwoDependent()
        {
            SMTVar.Reset();
            var p = new Problem("SolveTwoDependent");
            var x = new SMTVar("x", 0, 10);
            var y = new SMTVar("y", 0, 10);
            p.Assert(x < y);
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(SMTVar.Solve(p.Solve()));
                AssertValid(x);
                AssertValid(y);
                Assert.IsTrue(x.Value <= y.Value, $"Trial {i}: {x.Value} > {y.Value}");
            }
        }

        [TestMethod]
        public void PSMTNPCStatsTest()
        {
            SMTVar.Reset();
            var p = new Problem("NPC stats");
            var str = new SMTVar("str", 0, 10);
            var con = new SMTVar("con", 0, 10);
            var dex = new SMTVar("dex", 0, 10);
            var intel = new SMTVar("int", 0, 10);
            var wis = new SMTVar("wis", 0, 10);
            var charisma = new SMTVar("char", 0, 10);
            p.Unique("fighter", "magic user", "cleric", "thief");
            p.Assert(
                ((Expression)(Proposition)"fighter") > (str > intel),
                ((Expression)(Proposition)"fighter") > (str > 5),
                ((Expression)(Proposition)"fighter") > (con > 5),
                ((Expression)(Proposition)"fighter") > (intel < 8),
                ((Expression)(Proposition)"magic") > (str < intel),
                ((Expression)(Proposition)"magic") > (intel > 5),
                ((Expression)(Proposition)"magic") > (str < 8),
                ((Expression)(Proposition)"cleric") > (wis > 5),
                ((Expression)(Proposition)"cleric") > (con < wis),
                ((Expression)(Proposition)"theif") > (dex > 5),
                ((Expression)(Proposition)"theif") > (charisma > 5),
                ((Expression)(Proposition)"theif") > (wis < 5),
                ((Expression)(Proposition)"theif") > (dex > str),
                ((Expression)(Proposition)"theif") > (charisma > intel)
                );
            for (int i = 0; i < 100; i++)
            {
                Solution solution;
                do
                {
                    solution = p.Solve();
                } while (!SMTVar.Solve(solution));
            }
        }
    }
}
