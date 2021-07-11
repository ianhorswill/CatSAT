#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SATTests.cs" company="Ian Horswill">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using CatSAT;
using static CatSAT.Language;

namespace Tests
{
    [TestClass]
    public class PropagationTests
    {
        [TestMethod]
        public void BiconditionalTest()
        {
            var p = new Problem(nameof(BiconditionalTest));

            // Assert a <-> b
            void IfAndOnlyIf(Proposition a, Proposition b)
            {
                // a -> b
                p.AddClause(Not(a), b);
                // b -> a
                p.AddClause(Not(b), a);
            }
            
            // This only has two models: a=b=c=d=true, and a=b=c=d=false
            // This is the best-case for the Propagation step in BooleanSolver.MakeRandomAssignment
            // because once it chooses the value of the first variable, it should immediately propagate
            // to all other variables.
            IfAndOnlyIf("a", "b");
            IfAndOnlyIf("b", "c");
            IfAndOnlyIf("c", "d");

            var trueCount = 0;
            
            for (var i = 0; i < 100; i++)
            {
                var m = p.Solve();
                
                // This shouldn't have used any flips because propagation in initialization
                // should always produce a valid model
               Assert.AreEqual(0, p.SolveFlips.Max);
                
                // Make sure all variables have the same value
                if (m["a"])
                {
                    trueCount++;
                    Assert.IsTrue(m["b"]);
                    Assert.IsTrue(m["c"]);
                }
                else
                {
                    Assert.IsFalse(m["b"]);
                    Assert.IsFalse(m["c"]);
                }
            }

            // True and false models ought to be more or less equally likely.
            Assert.IsTrue(trueCount > 40);
            Assert.IsTrue(trueCount < 60);
        }

        /// <summary>
        /// used to test the number of calls of Flip() with new propagation
        /// </summary>
        [TestMethod]
        public void NormalClause1()
        {
            var p = new Problem("normal clause - short and more clauses");
            p.AddClause("b", "c");
            p.AddClause("a", "c");
            p.AddClause("d", "c");
            p.AddClause("e", "c");
            p.AddClause("f", "c");
            p.AddClause("g", "c");
            p.AddClause("h", "c");
            p.AddClause("i", "c");
            p.AddClause("j", "c");
            p.AddClause("k", "c");
            p.AddClause("l", "c");
            p.AddClause("m", "c");
            p.AddClause("n", "c");
            p.AddClause("o", "c");
            p.AddClause("p", "c");
            p.AddClause("q", "c");
            p.AddClause("r", "c");
            p.AddClause("s", "c");
            p.AddClause("t", "c");
            p.AddClause("u", "c");
            p.AddClause("v", "c");
            p.AddClause("w", "c");
            p.AddClause("x", "c");
            p.AddClause("y", "c");
            p.AddClause("z", "c");
            p.AddClause("a", "b");
            p.AddClause("d", "b");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average);  // average 0 flip 6 mswith propagation, 1 flip 7 ms w/o
        }
        [TestMethod]
        public void NormalClause2()
        {
            var p = new Problem("normal clause - short and more clauses2");
            p.AddClause("b", "c");
            p.AddClause("a", "d");
            p.AddClause("d", "c");
            p.AddClause("e", "c");
            p.AddClause("f", "g");
            p.AddClause("g", "c");
            p.AddClause("h", "k");
            p.AddClause("i", "c");
            p.AddClause("j", "g");
            p.AddClause("k", "c");
            p.AddClause("l", "c");
            p.AddClause("m", "c");
            p.AddClause("n", "r");
            p.AddClause("o", "c");
            p.AddClause("p", "c");
            p.AddClause("q", "c");
            p.AddClause("r", "s");
            p.AddClause("s", "c");
            p.AddClause("t", "c");
            p.AddClause("u", "a");
            p.AddClause("v", "c");
            p.AddClause("w", "c");
            p.AddClause("x", "c");
            p.AddClause("y", "t");
            p.AddClause("z", "c");
            p.AddClause("a", "b");
            p.AddClause("d", "b");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 0 flip 10ms with propagation, 3 flip 8ms w/o
        }

        [TestMethod]
        public void NormalClause3()
        {
            var prob = new Problem("normal clause - more disjuncts and less clauses");
            prob.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ll", "mm", "nn", "oo");
            prob.AddClause( "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "p", "q", "r", "s", "t", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "pp", "qq", "rr", "ss", "tt");
            prob.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "t", "u", "v", "w", "x", "y", "z", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo");
            prob.AddClause("a", "b", "c", "d", "e", "f", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "s", "t", "u", "v", "w", "x", "y", "z", "ab", "cd", "ac", "ad");
            prob.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo", "y", "z", "ab", "cd", "ac", "ad");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = prob.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 0 flip 10 ms with propagation, 0 flip 7 ms w/o
        }


        [TestMethod]
        public void NormalAndPbc1()
        {
            var p = new Problem("normal clauses mixed with PBCs");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ll", "mm", "nn", "oo");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "p", "q", "r", "s", "t", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "pp", "qq", "rr", "ss", "tt");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "t", "u", "v", "w", "x", "y", "z", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo");
            p.AddClause(1, 1, "e", "f");
            p.AddClause("w");
            p.AddClause(1, 1, "a", "b", "c");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 1 flip 12ms with propagation, 1 flip 26ms w/o
        }

        [TestMethod]
        public void NormalAndPbc2()
        {
            var p = new Problem("normal clauses mixed with small PBCs");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ll", "mm", "nn", "oo");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "p", "q", "r", "s", "t", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "pp", "qq", "rr", "ss", "tt");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i", "t", "u", "v", "w", "x", "y", "z", "aa", "bb", "cc", "dd", "ee", "ff", "gg", "hh", "ii", "jj", "kk", "ll", "mm", "nn", "oo");
            p.AddClause(1, 1, "e", "f");
            p.AddClause("w");
            p.AddConditionalClause(2, 2, "a", "a", "c");
            p.AddClause(1, 1, "b", "c");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 2 flips 15 ms with propagation, 2 flips 6 ms w/o
        }


        [TestMethod]
        public void NormalAndPbc3()
        {
            var p = new Problem("normal clauses mixed with large PBCs and conditional PBCs");
            p.AddClause("a", "b", "c", "d", "e", "f", "g", "h", "i");
            p.AddClause("a", "d");
            p.AddClause("d", "c");
            p.AddClause("f", "g");
            p.AddClause("g", Not("c"));
            p.AddClause(Not("j"), "g");
            p.AddClause("k", "c");
            p.AddClause("l", "c");
            p.AddClause(Not("m"), "j");
            p.AddClause("n", "r");
            p.AddClause(1, 1, "e", "f");
            p.AddClause("w");
            p.AddConditionalClause(2, 2, "a", "a", "c");
            p.AddConditionalClause(2, 2, "r", "r", "b");
            p.AddClause(1, 1, "b", "c");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 5 flips 6 ms w/ propagation, 8 flips 9 ms w/o
        }

        [TestMethod]
        public void NormalAndPbc4()
        {
            var p = new Problem("normal clauses mixed with large PBCs and conditional PBCs2");
            p.AddClause("b", "c");
            p.AddClause("a", "d");
            p.AddClause("d", "c");
            p.AddClause("e", "c");
            p.AddClause("f", "g");
            p.AddClause("g", "c");
            p.AddClause("h", "k");
            p.AddClause("i", "c");
            p.AddClause("j", "g");
            p.AddClause("k", "c");
            p.AddClause("l", "c");
            p.AddClause("m", "c");
            p.AddClause("n", "r");
            p.AddClause(1, 1, "e", "f");
            p.AddClause("w");
            p.AddConditionalClause(2, 2, "a", "a", "c");
            p.AddClause(1, 1, "b", "c");
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }

            int average = flip.Sum() / flip.Length;
            Console.WriteLine(average); // average 2 flip 10 ms w/ propagation, 4 flip 6 ms w/o
        }

        [TestMethod]
        public void SkipPropagationTest()
        {
            var p = new Problem("Skip last test's propagation during initialization");
            p.AddClause("b", "c");
            p.AddClause("a", "d");
            p.AddClause("d", "c");
            p.AddClause("e", "c");
            p.AddClause("f", "g");
            p.AddClause("g", "c");
            p.AddClause("h", "k");
            p.AddClause("i", "c");
            p.AddClause("j", "g");
            p.AddClause("k", "c");
            p.AddClause("l", "c");
            p.AddClause("m", "c");
            p.AddClause("n", "r");
            p.AddClause(1, 1, "e", "f");
            p.AddClause("w");
            p.AddConditionalClause(2, 2, "a", "a", "c");
            p.AddClause(1, 1, "b", "c");
            p.PropagateConstraintsDuringInitialization = false;
            int[] flip = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                flip[i] = m.Problem.BooleanSolver.SolveFlips;
            }
            int average = flip.Sum() / flip.Length;
            p.Assert(average == 4);
        }

        [TestMethod]
        public void UniqueConstraintPropagationTest()
        {
            // This should come out of the initialization process with a valid model
            // So this shouldn't require any flips
            var p = new Problem("normal clauses mixed with large PBCs and conditional PBCs2");
            p.AddClause(1, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m");

            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(0, m.Problem.BooleanSolver.SolveFlips);
            }
        }

        [TestMethod]
        public void RangeConstraintPropagationTest()
        {
            // This should come out of the initialization process with a valid model
            // So this shouldn't require any flips
            var p = new Problem("normal clauses mixed with large PBCs and conditional PBCs2");
            p.AddClause(3, 5, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m");

            for (int i = 0; i < 1000; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(0, m.Problem.BooleanSolver.SolveFlips);
            }
        }
    }
}
