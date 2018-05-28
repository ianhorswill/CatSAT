#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LanguageTests.cs" company="Ian Horswill">
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
using PicoSAT;
using static PicoSAT.Language;

namespace Tests
{
    [TestClass]
    public class LanguageTests
    {
        [TestMethod]
        public void ImplicationTest()
        {
            var p = new Problem("Implication test");
            var s = (Proposition)"s";
            var t = (Proposition)"t";

            p.Assert(
                (t & (Proposition) true) >= s,
                t
            );

            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.IsTrue(m.IsTrue(s));
                Assert.IsTrue(m.IsTrue(t));
            }
        }

        [TestMethod]
        public void BiconditionalTest()
        {
            var p = new Problem("Biconditional test");
            var s = (Proposition)"s";
            var t = (Proposition)"t";
            p.Assert(s == t);
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m.IsTrue(s), m.IsTrue(t));
            }
        }

        [TestMethod]
        public void BiconditionalTest2()
        {
            var p = new Problem("Biconditional test 2");
            var s = (Proposition)"s";
            var t = (Proposition)"t";
            var u = (Proposition) "u";
            p.Assert(s == (t&u));
            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m[s], m[t] && m[u]);
            }
        }

        [TestMethod]
        public void CompletionTest()
        {
            var p = new Problem("Completion test");

            var s = (Proposition)"s";
            var t = (Proposition)"t";
            var u = (Proposition)"u";

            var a = (Proposition)"a";
            var b = (Proposition)"b";

            var c = (Proposition)"c";

            p.Assert(
                s <= (t & u),
                s <= (a & b),
                s <= c
            );

            for (int i = 0; i < 100; i++)
            {
                var m = p.Solve();
                Assert.AreEqual(m.IsTrue(s),
                    (m.IsTrue(t) && m.IsTrue("u")) || (m.IsTrue(a) && m.IsTrue(b) || m.IsTrue(c)));
            }
        }

        [TestMethod]
        public void CallEqualTest()
        {
            Assert.AreEqual(new Call("foo", 1, 2), new Call("foo", 1, 2));
        }

        [TestMethod]
        public void CallHashTest()
        {
            Assert.AreEqual(new Call("foo", 1, 2).GetHashCode(), new Call("foo", 1, 2).GetHashCode());
        }

        [TestMethod]
        public void CallNotEqualTest()
        {
            Assert.AreNotEqual(new Call("foo", 1, 2), new Call("bar", 1, 2));
            Assert.AreNotEqual(new Call("foo", 1, 2), new Call("foo", 0, 2));
            Assert.AreNotEqual(new Call("foo", 1, 2), new Call("foo", 1, 0));
        }

        [TestMethod]
        public void ConstantFoldingTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Problem();

            Assert.AreEqual(Proposition.True, (Proposition)true & (Proposition)true);
            Assert.AreEqual(Proposition.False, (Proposition)false & (Proposition)true);
            Assert.AreEqual(Proposition.False, (Proposition)true & (Proposition)false);
            Assert.AreEqual(Proposition.False, (Proposition)false & (Proposition)false);

            var p = (Proposition) "p";

            Assert.AreEqual(p, (Proposition)true & p);
            Assert.AreEqual(Proposition.False, (Proposition)false & p);

            Assert.AreEqual(p, p & (Proposition)true);
            Assert.AreEqual(Proposition.False, p & (Proposition)false);

            Assert.AreEqual(Proposition.False, Not(true));
            Assert.AreEqual(Proposition.True, Not(false));
        }

        [TestMethod]
        public void FalseRuleTest()
        {
            var prog = new Problem("False rule test");
            var p = (Proposition) "p";
            prog.Assert( p <= false );
            prog.Solve();  // Force it to expand rule to completion

            // This should not generate any clauses
            Assert.AreEqual(0, prog.Clauses.Count);
        }

        [TestMethod]
        public void TrueRuleTest()
        {
            var prog = new Problem("True rule test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            prog.Assert(
                p <= true,
                p <= false,
                p <= q
            );
            var s = prog.Solve();  // Force it to expand rule to completion

            // This should have compiled to zero clauses but p should still always be true
            Assert.AreEqual(0, prog.Clauses.Count);
            Assert.IsTrue(s[p]);
        }

        [TestMethod]
        public void IgnoreFalseRuleTest()
        {
            var prog = new Problem("Ignore false rule test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            prog.Assert(
                p <= false,
                p <= q,
                p <= false
            );
            prog.Solve();  // Force it to expand rule to completion

            // This should have compiled to two clauses
            Assert.AreEqual(2, prog.Clauses.Count);
            // First clause should be q => p
            Assert.AreEqual(2, prog.Clauses[0].Disjuncts.Length);
            Assert.AreEqual(1, prog.Clauses[0].Disjuncts[0]);
            Assert.AreEqual(-2, prog.Clauses[0].Disjuncts[1]);
            // Section clause should be p => q
            Assert.AreEqual(2, prog.Clauses[1].Disjuncts.Length);
            Assert.AreEqual(-1, prog.Clauses[1].Disjuncts[0]);
            Assert.AreEqual(2, prog.Clauses[1].Disjuncts[1]);
        }

        [TestMethod]
        public void ContrapositiveTest()
        {
            var prog = new Problem("Contrapositive test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p)
            );
            for (int i = 0 ; i < 100; i++)
            {
                var m = prog.Solve();

                // Under completion semantics, only model should be the empty model.
                Assert.IsFalse(m.IsTrue(p));
                Assert.IsFalse(m.IsTrue(q));
                Assert.IsFalse(m.IsTrue(r));
            }
        }

        [TestMethod]
        public void OptimizationTest()
        {
            var prog = new Problem("Optimzer test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            var s = (Proposition) "s";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p)
            );
            prog.Optimize();
            Assert.IsTrue(prog.IsAlwaysFalse(p));
            Assert.IsTrue(prog.IsAlwaysFalse(q));
            Assert.IsTrue(prog.IsAlwaysFalse(r));
            Assert.IsFalse(prog.IsConstant(s));
        }

        [TestMethod, ExpectedException(typeof(ContradictionException))]
        public void ContradictionTest()
        {
            var prog = new Problem("Contradiction test");
            var p = (Proposition)"p";
            var q = (Proposition)"q";
            var r = (Proposition)"r";
            var s = (Proposition)"s";
            prog.Assert(
                p <= q,
                p <= r,
                Not(p),
                q
            );
            prog.Optimize();
            Assert.Fail();
        }
    }
}
